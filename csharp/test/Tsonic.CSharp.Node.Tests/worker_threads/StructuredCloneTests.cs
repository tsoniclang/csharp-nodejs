using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class StructuredCloneTests
{
    [Fact]
    public void SameNativeLongRetainsDistinctSourceCategoriesAcrossNestedCloneAndNativeBigInteger()
    {
        var source = new TsObject();
        source.WriteDynamicSlot("number", TsValue.fromsource<long, SourceNumberCategory<long>>(9007199254740994));
        source.WriteDynamicSlot("integer", TsValue.fromsource<long, SourceBigIntCategory<long>>(9007199254740994));
        var wide = System.Numerics.BigInteger.One << 300;
        source.WriteDynamicSlot("wide", TsValue.fromnative(wide));
        var copied = Decode(Encode(TsValue.fromnative(source)));

        Assert.Equal(SourceValueCategory.Number, copied.ReadDynamicSlot("number").Category);
        Assert.Equal(SourceValueCategory.BigInt, copied.ReadDynamicSlot("integer").Category);
        Assert.Equal(9007199254740994L, copied.ReadDynamicSlotAs<long>("number"));
        Assert.Equal(wide, copied.ReadDynamicSlotAs<System.Numerics.BigInteger>("wide"));
        Assert.False(TsValue.StrictEqual(copied.ReadDynamicSlot("number"), copied.ReadDynamicSlot("integer")));
    }

    private delegate TsValue DecodeValue(ReadOnlySpan<byte> payload);

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
    private static readonly Type CloneType = Type.GetType(
        "Tsonic.CSharp.Node.StructuredClone, Tsonic.CSharp.Node", throwOnError: true)!;
    private static readonly Func<TsValue, byte[]> Encode = CloneType
        .GetMethod("Encode", BindingFlags.Public | BindingFlags.Static)!
        .CreateDelegate<Func<TsValue, byte[]>>();
    private static readonly DecodeValue Decode = CloneType
        .GetMethod("Decode", BindingFlags.Public | BindingFlags.Static)!
        .CreateDelegate<DecodeValue>();

    [Fact]
    public void NumericWireCategoriesRejectBigIntFloatingCarriersButKeepExactIntegerRoles()
    {
        object[] values = [0.5d, 0.5f, (Half)0.5, 0.5m, 7d, 7m, double.NaN, double.PositiveInfinity, double.NegativeInfinity];
        foreach (var value in values)
        {
            var payload = Encode(TsValue.fromnative(value));
            Assert.Equal((byte)SourceValueCategory.Number, payload[1]);
            var decoded = Decode(payload);
            Assert.Equal(SourceValueCategory.Number, decoded.Category);
            Assert.Equal(value, decoded.unwrap());
            var contradictory = (byte[])payload.Clone();
            contradictory[1] = (byte)SourceValueCategory.BigInt;
            Assert.Throws<InvalidOperationException>(() => Decode(contradictory));
        }
        foreach (var category in new[] { SourceValueCategory.Number, SourceValueCategory.BigInt })
            foreach (var value in new[] { long.MinValue, 0L, 9_007_199_254_740_994L, long.MaxValue })
            {
                var decoded = Decode(Encode(TsValue.fromsource(value, category)));
                Assert.Equal(category, decoded.Category);
                Assert.Equal(value, Assert.IsType<long>(decoded.unwrap()));
            }
    }

    [Fact]
    public void NativeNumbersPreserveBitsInsideRepeatedAndCyclicObjects()
    {
        var record = new TsObject();
        record.WriteDynamicSlot("exact", 9_007_199_254_740_993L);
        record.WriteDynamicSlot("maximum", UInt128.MaxValue);
        record.WriteDynamicSlot("self", record);
        var aliases = new TsArray();
        aliases.WriteDynamicElement(0, record);
        aliases.WriteDynamicElement(1, record);
        var copied = Assert.IsType<TsArray>(Decode(Encode(TsValue.fromnative(aliases))).unwrap());
        var first = Assert.IsType<TsObject>(copied.ReadDynamicElement(0).unwrap());
        Assert.NotSame(record, first);
        Assert.Same(first, copied.ReadDynamicElement(1).unwrap());
        Assert.Same(first, first.ReadDynamicSlot("self").unwrap());
        Assert.Equal(9_007_199_254_740_993L, Assert.IsType<long>(first.ReadDynamicSlot("exact").unwrap()));
        Assert.Equal(UInt128.MaxValue, Assert.IsType<UInt128>(first.ReadDynamicSlot("maximum").unwrap()));
    }

    [Fact]
    public void FloatingCarriersRetainSignedZeroAndNanPayloads()
    {
        foreach (var bits in new[] { 0x8000000000000000UL, 0x7ff8000000000001UL, 0xfff0000000000000UL })
        {
            var value = BitConverter.UInt64BitsToDouble(bits);
            var actual = Assert.IsType<double>(Decode(Encode(TsValue.fromnative(value))).unwrap());
            Assert.Equal(bits, BitConverter.DoubleToUInt64Bits(actual));
        }
        foreach (var bits in new[] { 0x80000000U, 0x7fc00001U, 0xff800000U })
        {
            var value = BitConverter.UInt32BitsToSingle(bits);
            var actual = Assert.IsType<float>(Decode(Encode(TsValue.fromnative(value))).unwrap());
            Assert.Equal(bits, BitConverter.SingleToUInt32Bits(actual));
        }
    }

    [Fact]
    public void NativeUnionProjectionPreservesExactPayloadsAndNestedAliasIdentity()
    {
        var array = new Tsonic.CSharp.Js.JSArray<Union<string, ulong>>(new[] {
            Union<string, ulong>.From1("ready"), Union<string, ulong>.From2(ulong.MaxValue),
        });
        var aliases = new TsArray();
        aliases.WriteDynamicElement(0, Union<int, object>.From2(array));
        aliases.WriteDynamicElement(1, Union<object, string>.From1(array));
        var copied = Assert.IsType<TsArray>(Decode(Encode(TsValue.fromnative(aliases))).unwrap());
        var first = copied.ReadDynamicElement(0);
        Assert.True(first.IsArray());
        Assert.Same(first.unwrap(), copied.ReadDynamicElement(1).unwrap());
        Assert.Equal("ready", first.ReadDynamicElement(0).unwrap());
        Assert.Equal(ulong.MaxValue, Assert.IsType<ulong>(first.ReadDynamicElement(1).unwrap()));
    }

    [Fact]
    public void EveryNativeNumericPayloadRejectsTruncationAndTrailingBytes()
    {
        object[] values = [(byte)255, (sbyte)-128, short.MinValue, ushort.MaxValue,
            int.MinValue, uint.MaxValue, long.MinValue, ulong.MaxValue,
            nint.MinValue, nuint.MaxValue, Int128.MinValue, UInt128.MaxValue,
            Half.MaxValue, float.Epsilon, double.Epsilon, decimal.MaxValue];
        foreach (var value in values)
        {
            var payload = Encode(TsValue.fromnative(value));
            for (var length = 0; length < payload.Length; length++)
            {
                var truncated = payload.AsSpan(0, length).ToArray();
                Assert.Throws<EndOfStreamException>(() => Decode(truncated));
            }
            byte[] trailing = [.. payload, 0];
            Assert.Throws<InvalidOperationException>(() => Decode(trailing));
        }
        Assert.Throws<InvalidOperationException>(() => Decode(new byte[] { 1, 255 }));
        Assert.Throws<InvalidOperationException>(() => Decode(new byte[] { 255, 0 }));
    }
}
