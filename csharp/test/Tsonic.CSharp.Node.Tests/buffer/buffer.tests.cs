using Xunit;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node.Tests;

public partial class BufferTests
{
    [Fact]
    public void alloc_ShouldCreateZeroFilledBuffer()
    {
        var buf = Buffer.alloc(10);
        Assert.Equal(10, buf.length);
        Assert.All(Enumerable.Range(0, 10), i => Assert.Equal(0, buf[i]));
    }

    [Fact]
    public void alloc_WithFillValue_ShouldFillBuffer()
    {
        var buf = Buffer.alloc(5, 42);
        Assert.All(Enumerable.Range(0, 5), i => Assert.Equal(42, buf[i]));
    }

    [Fact]
    public void allocUnsafe_ShouldCreateBuffer()
    {
        var buf = Buffer.allocUnsafe(10);
        Assert.Equal(10, buf.length);
    }

    [Fact]
    public void from_String_ShouldCreateBufferFromString()
    {
        var buf = Buffer.from("hello", "utf8");
        Assert.Equal(5, buf.length);
        Assert.Equal("hello", buf.toString());
    }

    [Fact]
    public void from_Array_ShouldCreateBufferFromArray()
    {
        var buf = Buffer.from(new[] { 1, 2, 3, 4, 5 });
        Assert.Equal(5, buf.length);
        Assert.Equal(1, buf[0]);
        Assert.Equal(5, buf[4]);
    }

    [Fact]
    public void from_ArrayWithLargeValues_ShouldTruncateTo8Bits()
    {
        var buf = Buffer.from(new[] { 257, 258 });
        Assert.Equal(1, buf[0]); // 257 & 0xFF = 1
        Assert.Equal(2, buf[1]); // 258 & 0xFF = 2
    }

    [Fact]
    public void of_ShouldCreateBufferFromVariadicArgs()
    {
        var buf = Buffer.of(1, 2, 3);
        Assert.Equal(3, buf.length);
        Assert.Equal(1, buf[0]);
        Assert.Equal(3, buf[2]);
    }

    [Fact]
    public void isBuffer_ShouldReturnTrueForBuffer()
    {
        var buf = Buffer.alloc(10);
        Assert.True(Buffer.isBuffer(buf));
    }

    [Fact]
    public void isBuffer_ShouldReturnFalseForNonBuffer()
    {
        Assert.False(Buffer.isBuffer("string"));
        Assert.False(Buffer.isBuffer(42));
        Assert.False(Buffer.isBuffer(null));
    }

    [Fact]
    public void isEncoding_ShouldReturnTrueForValidEncodings()
    {
        Assert.True(Buffer.isEncoding("utf8"));
        Assert.True(Buffer.isEncoding("utf-8"));
        Assert.True(Buffer.isEncoding("ascii"));
        Assert.True(Buffer.isEncoding("base64"));
        Assert.True(Buffer.isEncoding("hex"));
    }

    [Fact]
    public void isEncoding_ShouldReturnFalseForInvalidEncoding()
    {
        Assert.False(Buffer.isEncoding("invalid"));
    }

    [Fact]
    public void byteLength_ShouldCalculateCorrectLength()
    {
        Assert.Equal(5, Buffer.byteLength("hello", "utf8"));
        Assert.Equal(5, Buffer.byteLength("½+¼", "utf8")); // ½ is 2 bytes, + is 1 byte, ¼ is 2 bytes
    }

    [Fact]
    public void concat_ShouldConcatenateBuffers()
    {
        var buf1 = Buffer.from("hello");
        var buf2 = Buffer.from(" world");
        var result = Buffer.concat(new[] { buf1, buf2 });

        Assert.Equal("hello world", result.toString());
    }

    [Fact]
    public void concat_WithTotalLength_ShouldLimitSize()
    {
        var buf1 = Buffer.from("hello");
        var buf2 = Buffer.from(" world");
        var result = Buffer.concat(new[] { buf1, buf2 }, 8);

        Assert.Equal(8, result.length);
        Assert.Equal("hello wo", result.toString());
    }

    [Fact]
    public void toString_Utf8_ShouldDecodeCorrectly()
    {
        var buf = Buffer.from("hello");
        Assert.Equal("hello", buf.toString("utf8"));
    }

    [Fact]
    public void toString_Hex_ShouldEncodeAsHex()
    {
        var buf = Buffer.from(new byte[] { 0x48, 0x65, 0x6c, 0x6c, 0x6f });
        Assert.Equal("48656c6c6f", buf.toString("hex"));
    }

    [Fact]
    public void toString_Base64_ShouldEncodeAsBase64()
    {
        var buf = Buffer.from("hello");
        Assert.Equal("aGVsbG8=", buf.toString("base64"));
    }

    [Fact]
    public void toString_WithRange_ShouldDecodeSubstring()
    {
        var buf = Buffer.from("hello world");
        Assert.Equal("world", buf.toString("utf8", 6, 11));
    }

    [Fact]
    public void write_ShouldWriteString()
    {
        var buf = Buffer.alloc(10);
        var written = buf.write("hello", 0);

        Assert.Equal(5, written);
        Assert.Equal("hello", buf.toString("utf8", 0, 5));
    }

    [Fact]
    public void write_WithOffset_ShouldWriteAtOffset()
    {
        var buf = Buffer.alloc(10);
        buf.write("hello", 5);

        Assert.Equal("hello", buf.toString("utf8", 5, 10));
    }

    [Fact]
    public void write_Hex_ShouldWriteHexString()
    {
        var buf = Buffer.alloc(5);
        buf.write("48656c6c6f", 0, null, "hex");

        Assert.Equal("Hello", buf.toString("utf8"));
    }

    [Fact]
    public void slice_ShouldCreateSubBuffer()
    {
        var buf = Buffer.from("hello world");
        var slice = buf.slice(6, 11);

        Assert.Equal(5, slice.length);
        Assert.Equal("world", slice.toString());
    }

    [Fact]
    public void slice_WithNegativeIndices_ShouldHandleCorrectly()
    {
        var buf = Buffer.from("hello");
        var slice = buf.slice(-3);

        Assert.Equal("llo", slice.toString());
    }

    [Fact]
    public void copy_ShouldCopyBytes()
    {
        var source = Buffer.from("hello");
        var target = Buffer.alloc(10);

        var copied = source.copy(target, 0, 0, 5);

        Assert.Equal(5, copied);
        Assert.Equal("hello", target.toString("utf8", 0, 5));
    }

    [Fact]
    public void copy_WithOffset_ShouldCopyToOffset()
    {
        var source = Buffer.from("world");
        var target = Buffer.alloc(11); // 11 bytes total
        Buffer.from("hello").copy(target, 0); // Copy "hello" to start
        target[5] = (byte)' '; // Add space

        source.copy(target, 6); // Copy "world" to offset 6

        Assert.Equal("hello world", target.toString("utf8"));
    }

    [Fact]
    public void fill_WithNumber_ShouldFillBuffer()
    {
        var buf = Buffer.alloc(5);
        buf.fill(42);

        Assert.All(Enumerable.Range(0, 5), i => Assert.Equal(42, buf[i]));
    }

    [Fact]
    public void fill_AndAlloc_ShouldUseExactUint8ConversionForEveryNativeNumericWidth()
    {
        var cases = new (object value, byte expected)[]
        {
            ((sbyte)-1, 255), ((byte)7, 7), ((short)-257, 255), ((ushort)65535, 255),
            (257, 1), (uint.MaxValue, 255), (9007199254740993L, 1), (long.MinValue, 0),
            (ulong.MaxValue, 255), (1.5F, 1), (-257.75F, 255), (float.NaN, 0),
            (float.PositiveInfinity, 0), (float.NegativeInfinity, 0),
            (1.5D, 1), (-1D, 255), (256D, 0), (-257.75D, 255), (double.NaN, 0),
            (double.PositiveInfinity, 0), (double.NegativeInfinity, 0),
            (9007199254740993.75M, 1), (decimal.MaxValue, 255), (decimal.MinValue, 1),
            ((Half)1.5F, 1), (Half.NaN, 0), (Half.PositiveInfinity, 0), (Half.NegativeInfinity, 0)
        };
        foreach (var (value, expected) in cases)
        {
            var buffer = Buffer.alloc(4, 9);
            Assert.Same(buffer, buffer.fill(value, 1, 3));
            Assert.Equal(9, buffer[0]);
            Assert.Equal(expected, buffer[1]);
            Assert.Equal(expected, buffer[2]);
            Assert.Equal(9, buffer[3]);
            var allocated = Buffer.alloc(3, value);
            Assert.All(Enumerable.Range(0, 3), index => Assert.Equal(expected, allocated[index]));
        }
    }

    [Fact]
    public void fill_AndAlloc_ShouldRefuseUnsupportedValuesEvenForAnEmptyRange()
    {
        var values = new object[] { true, false, new object(), new byte[] { 1 }, new Tsonic.CSharp.Js.Uint8Array(1) };
        foreach (var value in values)
        {
            var buffer = Buffer.alloc(3, 9);
            var fillError = Assert.Throws<TypeError>(() => buffer.fill(value));
            Assert.Contains("ERR_INVALID_ARG_TYPE", fillError.Message);
            Assert.All(Enumerable.Range(0, 3), index => Assert.Equal(9, buffer[index]));
            Assert.Contains("ERR_INVALID_ARG_TYPE", Assert.Throws<TypeError>(() => buffer.fill(value, 0, 0)).Message);
            Assert.Contains("ERR_INVALID_ARG_TYPE", Assert.Throws<TypeError>(() => Buffer.alloc(3, value)).Message);
            Assert.Contains("ERR_INVALID_ARG_TYPE", Assert.Throws<TypeError>(() => Buffer.alloc(0, value)).Message);
        }
    }

    [Fact]
    public void fill_WithString_ShouldFillWithPattern()
    {
        var buf = Buffer.alloc(10);
        buf.fill("ab");

        Assert.Equal("ababababab", buf.toString());
    }

    [Fact]
    public void fill_WithRange_ShouldFillRange()
    {
        var buf = Buffer.alloc(10);
        buf.fill(42, 2, 7);

        Assert.Equal(0, buf[0]);
        Assert.Equal(42, buf[2]);
        Assert.Equal(42, buf[6]);
        Assert.Equal(0, buf[7]);
    }

    [Fact]
    public void equals_ShouldReturnTrueForEqualBuffers()
    {
        var buf1 = Buffer.from("hello");
        var buf2 = Buffer.from("hello");

        Assert.True(buf1.equals(buf2));
    }

    [Fact]
    public void equals_ShouldReturnFalseForDifferentBuffers()
    {
        var buf1 = Buffer.from("hello");
        var buf2 = Buffer.from("world");

        Assert.False(buf1.equals(buf2));
    }

    [Fact]
    public void compare_ShouldReturnZeroForEqualBuffers()
    {
        var buf1 = Buffer.from("abc");
        var buf2 = Buffer.from("abc");

        Assert.Equal(0, buf1.compare(buf2));
    }

    [Fact]
    public void compare_ShouldReturnNegativeWhenFirstIsLess()
    {
        var buf1 = Buffer.from("abc");
        var buf2 = Buffer.from("abd");

        Assert.Equal(-1, buf1.compare(buf2));
    }

    [Fact]
    public void compare_ShouldReturnPositiveWhenFirstIsGreater()
    {
        var buf1 = Buffer.from("abd");
        var buf2 = Buffer.from("abc");

        Assert.Equal(1, buf1.compare(buf2));
    }

    [Fact]
    public void indexOf_ShouldFindValue()
    {
        var buf = Buffer.from("hello world");
        Assert.Equal(6, buf.indexOf("world"));
        Assert.Equal(2, buf.indexOf((int)'l')); // Pass as int
    }

    [Fact]
    public void indexOf_ShouldReturnMinusOneWhenNotFound()
    {
        var buf = Buffer.from("hello");
        Assert.Equal(-1, buf.indexOf("xyz"));
    }

    [Fact]
    public void lastIndexOf_ShouldFindLastOccurrence()
    {
        var buf = Buffer.from("hello world hello");
        Assert.Equal(12, buf.lastIndexOf("hello"));
    }

    [Fact]
    public void includes_ShouldReturnTrueWhenFound()
    {
        var buf = Buffer.from("hello world");
        Assert.True(buf.includes("world"));
    }

    [Fact]
    public void includes_ShouldReturnFalseWhenNotFound()
    {
        var buf = Buffer.from("hello");
        Assert.False(buf.includes("xyz"));
    }

    [Fact]
    public void reverse_ShouldReverseBuffer()
    {
        var buf = Buffer.from("hello");
        buf.reverse();

        Assert.Equal("olleh", buf.toString());
    }

    [Fact]
    public void swap16_ShouldSwapBytes()
    {
        var buf = Buffer.from(new byte[] { 0x01, 0x02, 0x03, 0x04 });
        buf.swap16();

        Assert.Equal(0x02, buf[0]);
        Assert.Equal(0x01, buf[1]);
        Assert.Equal(0x04, buf[2]);
        Assert.Equal(0x03, buf[3]);
    }


    [Fact]
    public void toJSON_ShouldReturnCorrectFormat()
    {
        var buf = Buffer.from(new[] { 1, 2, 3 });
        var json = buf.toJSON();

        // Verify it's an object (toJSON returns an anonymous type)
        Assert.NotNull(json);

        // Verify the ToString() representation suggests it's an object with properties
        var str = json.ToString();
        Assert.NotNull(str);
        Assert.NotEmpty(str!);
    }

    [Fact]
    public void Indexer_ShouldAllowReadWrite()
    {
        var buf = Buffer.alloc(3);
        buf[0] = 1;
        buf[1] = 2;
        buf[2] = 3;

        Assert.Equal(1, buf[0]);
        Assert.Equal(2, buf[1]);
        Assert.Equal(3, buf[2]);
    }
}
