using System.Collections;
using System.Numerics;
using System.Text;
using Tsonic.CSharp.Js;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

internal static partial class StructuredClone
{
    private const byte FormatVersion = 2;
    private const int MaximumDepth = 256;
    private const int MaximumEntries = 1 << 20;
    private const int MaximumStringCodeUnits = 1 << 24;

    public static byte[] Encode(TsValue value)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(FormatVersion);
        WriteValue(writer, value, new EncodingState(), 0);
        writer.Flush();
        if (stream.Length > WorkerTransport.MaximumFrameBytes)
            throw DataCloneError("Structured-clone payload exceeds the finite transport limit.");
        return stream.ToArray();
    }

    public static TsValue Decode(ReadOnlySpan<byte> payload)
    {
        using var stream = new MemoryStream(payload.ToArray(), writable: false);
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        if (reader.ReadByte() != FormatVersion)
            throw DataCloneError("Structured-clone payload version is unsupported.");
        var value = ReadValue(reader, new DecodingState(), 0);
        if (stream.Position != stream.Length)
            throw DataCloneError("Structured-clone payload contains trailing data.");
        return TsValue.fromnative(value);
    }

    private static void WriteValue<TValue>(
        BinaryWriter writer,
        TValue original,
        EncodingState state,
        int depth)
    {
        if (depth > MaximumDepth)
            throw DataCloneError("Structured-clone depth exceeds the finite limit.");
        var stored = TsValue.fromnative(original);
        writer.Write((byte)stored.Category);
        var value = Unwrap(stored.unwrap());
        switch (value)
        {
            case null:
                writer.Write((byte)1);
                return;
            case false:
                writer.Write((byte)2);
                return;
            case true:
                writer.Write((byte)3);
                return;
            case double number:
                writer.Write((byte)4);
                writer.Write(number);
                return;
            case byte number:
                writer.Write((byte)10);
                writer.Write(number);
                return;
            case sbyte number:
                writer.Write((byte)11);
                writer.Write(number);
                return;
            case short number:
                writer.Write((byte)12);
                writer.Write(number);
                return;
            case ushort number:
                writer.Write((byte)13);
                writer.Write(number);
                return;
            case int number:
                writer.Write((byte)14);
                writer.Write(number);
                return;
            case uint number:
                writer.Write((byte)15);
                writer.Write(number);
                return;
            case long number:
                writer.Write((byte)16);
                writer.Write(number);
                return;
            case ulong number:
                writer.Write((byte)17);
                writer.Write(number);
                return;
            case float number:
                writer.Write((byte)18);
                writer.Write(number);
                return;
            case decimal number:
                writer.Write((byte)19);
                writer.Write(number);
                return;
            case nint number:
                writer.Write((byte)20);
                writer.Write((long)number);
                return;
            case nuint number:
                writer.Write((byte)21);
                writer.Write((ulong)number);
                return;
            case Int128 number:
                writer.Write((byte)22);
                WriteWideInteger(writer, unchecked((UInt128)number));
                return;
            case UInt128 number:
                writer.Write((byte)23);
                WriteWideInteger(writer, number);
                return;
            case Half number:
                writer.Write((byte)24);
                writer.Write(BitConverter.HalfToUInt16Bits(number));
                return;
            case BigInteger number:
                var bytes = number.ToByteArray();
                Reserve(bytes.Length, state);
                writer.Write((byte)25);
                writer.Write(bytes.Length);
                writer.Write(bytes);
                return;
            case string text:
                writer.Write((byte)5);
                WriteString(writer, text, state);
                return;
            case Buffer buffer:
                WriteBuffer(writer, buffer, state);
                return;
            case TsObject target:
                WriteObject(writer, target, target, state, depth);
                return;
            case JSObject target:
                WriteObject(
                    writer,
                    target,
                    target.entries().Select(entry =>
                        new KeyValuePair<string, object?>(entry.key, entry.value)),
                    state,
                    depth);
                return;
            case IReadOnlyDictionary<string, object?> target:
                WriteObject(writer, target, target, state, depth);
                return;
            case IDictionary<string, object?> target:
                WriteObject(writer, target, target, state, depth);
                return;
            case TsArray target:
                WriteArray(
                    writer,
                    target,
                    target.length,
                    StoredArrayEntries(target),
                    state,
                    depth);
                return;
            case IDynamicArray target:
                WriteArray(
                    writer,
                    target,
                    target.Length,
                    DynamicArrayEntries(stored, target),
                    state,
                    depth);
                return;
            default:
                throw DataCloneError(
                    $"Value carrier '{value.GetType().FullName}' is not structured-cloneable.");
        }
    }

    private static void WriteObject<TValue>(
        BinaryWriter writer,
        object identity,
        IEnumerable<KeyValuePair<string, TValue>> entries,
        EncodingState state,
        int depth)
    {
        if (WriteReference(writer, identity, state))
            return;
        var values = entries.ToArray();
        Reserve(values.Length + 1, state);
        writer.Write((byte)6);
        writer.Write(state.Identities[identity]);
        writer.Write(values.Length);
        foreach (var entry in values)
        {
            WriteString(writer, entry.Key, state);
            WriteValue(writer, entry.Value, state, depth + 1);
        }
    }

    private static void WriteArray<TValue>(
        BinaryWriter writer,
        object identity,
        int length,
        IEnumerable<(int Index, TValue Value)> entries,
        EncodingState state,
        int depth)
    {
        if (length < 0)
            throw DataCloneError("Structured-clone array length is negative.");
        if (WriteReference(writer, identity, state))
            return;
        var values = entries.ToArray();
        Reserve(1, state);
        Reserve(length, state);
        writer.Write((byte)7);
        writer.Write(state.Identities[identity]);
        writer.Write(length);
        writer.Write(values.Length);
        var indexes = new HashSet<int>();
        foreach (var entry in values)
        {
            if (entry.Index < 0 || entry.Index >= length || !indexes.Add(entry.Index))
                throw DataCloneError("Structured-clone array entry is outside its declared shape.");
            writer.Write(entry.Index);
            WriteValue(writer, entry.Value, state, depth + 1);
        }
    }

    private static void WriteBuffer(
        BinaryWriter writer,
        Buffer buffer,
        EncodingState state)
    {
        if (WriteReference(writer, buffer, state))
            return;
        Reserve(1, state);
        Reserve(buffer.length, state);
        writer.Write((byte)8);
        writer.Write(state.Identities[buffer]);
        writer.Write(buffer.length);
        writer.Write(buffer.InternalMemory.Span);
    }

    private static bool WriteReference(
        BinaryWriter writer,
        object identity,
        EncodingState state)
    {
        if (state.Identities.TryGetValue(identity, out var existing))
        {
            writer.Write((byte)9);
            writer.Write(existing);
            return true;
        }
        var index = state.Identities.Count;
        if (index >= MaximumEntries)
            throw DataCloneError("Structured-clone container count exceeds the finite limit.");
        state.Identities.Add(identity, index);
        return false;
    }

    private static TsValue ReadValue(
        BinaryReader reader,
        DecodingState state,
        int depth)
    {
        if (depth > MaximumDepth)
            throw DataCloneError("Structured-clone depth exceeds the finite limit.");
        var category = (SourceValueCategory)reader.ReadByte();
        if (category > SourceValueCategory.Function)
            throw DataCloneError("Structured-clone payload contains an unknown value category.");
        var tag = reader.ReadByte();
        object? value = tag switch
        {
            1 => null,
            2 => false,
            3 => true,
            4 => reader.ReadDouble(),
            5 => ReadString(reader, state),
            6 => ReadObject(reader, state, depth),
            7 => ReadArray(reader, state, depth),
            8 => ReadBuffer(reader, state),
            9 => ReadReference(reader, state),
            10 => reader.ReadByte(),
            11 => reader.ReadSByte(),
            12 => reader.ReadInt16(),
            13 => reader.ReadUInt16(),
            14 => reader.ReadInt32(),
            15 => reader.ReadUInt32(),
            16 => reader.ReadInt64(),
            17 => reader.ReadUInt64(),
            18 => reader.ReadSingle(),
            19 => reader.ReadDecimal(),
            20 => checked((nint)reader.ReadInt64()),
            21 => checked((nuint)reader.ReadUInt64()),
            22 => unchecked((Int128)ReadWideInteger(reader)),
            23 => ReadWideInteger(reader),
            24 => BitConverter.UInt16BitsToHalf(reader.ReadUInt16()),
            25 => ReadBigInteger(reader, state),
            _ => throw DataCloneError("Structured-clone payload contains an unknown value tag."),
        };
        var valid = tag switch
        {
            1 => category == SourceValueCategory.Absent,
            2 or 3 => category == SourceValueCategory.Boolean,
            5 => category == SourceValueCategory.String,
            6 or 7 or 8 or 9 => category == SourceValueCategory.Object,
            4 or 18 or 19 or 24 => category == SourceValueCategory.Number,
            10 or 11 or 12 or 13 or 14 or 15 or 16 or 17 or 20 or 21 or 22 or 23 or 25 =>
                category is SourceValueCategory.Number or SourceValueCategory.BigInt,
            _ => false,
        };
        if (!valid) throw DataCloneError("Structured-clone category does not match its value domain.");
        return TsValue.fromsource(value, category);
    }

    private static TsObject ReadObject(
        BinaryReader reader,
        DecodingState state,
        int depth)
    {
        var result = new TsObject();
        RegisterContainer(reader, state, result);
        var count = ReadCount(reader, state);
        var keys = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < count; index++)
        {
            var key = ReadString(reader, state);
            if (!keys.Add(key))
                throw DataCloneError("Structured-clone object contains a duplicate key.");
            result.WriteDynamicSlot(key, ReadValue(reader, state, depth + 1));
        }
        return result;
    }

    private static TsArray ReadArray(
        BinaryReader reader,
        DecodingState state,
        int depth)
    {
        var result = new TsArray();
        RegisterContainer(reader, state, result);
        var length = reader.ReadInt32();
        if (length < 0)
            throw DataCloneError("Structured-clone array length is negative.");
        Reserve(length, state);
        var count = reader.ReadInt32();
        if (count < 0 || count > length)
            throw DataCloneError("Structured-clone array entry count is invalid.");
        result.WriteDynamicSlot("length", length);
        var indexes = new HashSet<int>();
        for (var entry = 0; entry < count; entry++)
        {
            var index = reader.ReadInt32();
            if (index < 0 || index >= length || !indexes.Add(index))
                throw DataCloneError("Structured-clone array entry is outside its declared shape.");
            result.WriteDynamicElement(index, ReadValue(reader, state, depth + 1));
        }
        return result;
    }

    private static Buffer ReadBuffer(BinaryReader reader, DecodingState state)
    {
        var identity = ReadNewContainerIdentity(reader, state);
        var length = reader.ReadInt32();
        if (length < 0 || length > WorkerTransport.MaximumFrameBytes)
            throw DataCloneError("Structured-clone Buffer length exceeds the finite limit.");
        Reserve(length, state);
        var bytes = reader.ReadBytes(length);
        if (bytes.Length != length)
            throw new EndOfStreamException("Structured-clone Buffer is truncated.");
        var result = Buffer.from(bytes);
        state.Containers.Add(result);
        if (identity != state.Containers.Count - 1)
            throw DataCloneError("Structured-clone container identity is out of order.");
        return result;
    }

    private static object ReadReference(BinaryReader reader, DecodingState state)
    {
        var identity = reader.ReadInt32();
        if (identity < 0 || identity >= state.Containers.Count)
            throw DataCloneError("Structured-clone reference is outside the container table.");
        return state.Containers[identity];
    }

    private static void RegisterContainer(
        BinaryReader reader,
        DecodingState state,
        object container)
    {
        var identity = ReadNewContainerIdentity(reader, state);
        state.Containers.Add(container);
        if (identity != state.Containers.Count - 1)
            throw DataCloneError("Structured-clone container identity is out of order.");
        Reserve(1, state);
    }

    private static int ReadNewContainerIdentity(
        BinaryReader reader,
        DecodingState state)
    {
        var identity = reader.ReadInt32();
        if (identity != state.Containers.Count || identity >= MaximumEntries)
            throw DataCloneError("Structured-clone container identity is out of order.");
        return identity;
    }

    private static int ReadCount(BinaryReader reader, DecodingState state)
    {
        var count = reader.ReadInt32();
        if (count < 0)
            throw DataCloneError("Structured-clone entry count is negative.");
        Reserve(count, state);
        return count;
    }

    private static void Reserve(int count, EncodingState state)
    {
        if (count < 0 || count > MaximumEntries - state.Entries)
            throw DataCloneError("Structured-clone entry count exceeds the finite limit.");
        state.Entries += count;
    }

    private static void Reserve(int count, DecodingState state)
    {
        if (count < 0 || count > MaximumEntries - state.Entries)
            throw DataCloneError("Structured-clone entry count exceeds the finite limit.");
        state.Entries += count;
    }

    private static object? Unwrap(object? value)
    {
        return TsValue.UnwrapClosedValue(value);
    }

    private static IEnumerable<(int Index, TsValue Value)> DynamicArrayEntries(
        TsValue source, IDynamicArray target)
    {
        for (var index = 0; index < target.Length; index++)
            if (target.HasIndex(index))
                yield return (index, source.ReadDynamicSlot(index.ToString(System.Globalization.CultureInfo.InvariantCulture)));
    }

    private static IEnumerable<(int Index, TsValue Value)> StoredArrayEntries(TsArray target)
    {
        var storage = (IDynamicArray)target;
        for (var index = 0; index < target.length; index++)
            if (storage.HasIndex(index)) yield return (index, target.ReadDynamicElement(index));
    }

    private static InvalidOperationException DataCloneError(string message) =>
        new($"DATA_CLONE_ERR: {message}");

    private sealed class EncodingState
    {
        public Dictionary<object, int> Identities { get; } =
            new(ReferenceEqualityComparer.Instance);
        public int Entries { get; set; }
        public int StringCodeUnits { get; set; }
    }

    private sealed class DecodingState
    {
        public List<object> Containers { get; } = [];
        public int Entries { get; set; }
        public int StringCodeUnits { get; set; }
    }
}
