using System.Collections;
using System.Globalization;
using System.Text;
using Tsonic.CSharp.Js;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

internal static class StructuredClone
{
    private const byte FormatVersion = 1;
    private const int MaximumDepth = 256;
    private const int MaximumEntries = 1 << 20;
    private const int MaximumStringCodeUnits = 1 << 24;

    public static byte[] Encode(TsValue value)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(FormatVersion);
        WriteValue(writer, Unwrap(value.unwrap()), new EncodingState(), 0);
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
        return TsValue.from(value);
    }

    private static void WriteValue(
        BinaryWriter writer,
        object? value,
        EncodingState state,
        int depth)
    {
        if (depth > MaximumDepth)
            throw DataCloneError("Structured-clone depth exceeds the finite limit.");
        value = Unwrap(value);
        switch (value)
        {
            case Undefined:
                writer.Write((byte)0);
                return;
            case null:
                writer.Write((byte)1);
                return;
            case false:
                writer.Write((byte)2);
                return;
            case true:
                writer.Write((byte)3);
                return;
            case byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal:
                writer.Write((byte)4);
                writer.Write(Convert.ToDouble(value, CultureInfo.InvariantCulture));
                return;
            case string text:
                writer.Write((byte)5);
                WriteString(writer, text, state);
                return;
            case Buffer buffer:
                WriteBuffer(writer, buffer, state);
                return;
            case TsObject target:
                WriteObject(writer, target, target.entries(), state, depth);
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
                    target.entries().Select(entry =>
                        (int.Parse(entry.Key, CultureInfo.InvariantCulture), entry.Value)),
                    state,
                    depth);
                return;
            case IDynamicArray target:
                WriteArray(
                    writer,
                    target,
                    target.Length,
                    DynamicArrayEntries(target),
                    state,
                    depth);
                return;
            default:
                throw DataCloneError(
                    $"Value carrier '{value.GetType().FullName}' is not structured-cloneable.");
        }
    }

    private static void WriteObject(
        BinaryWriter writer,
        object identity,
        IEnumerable<KeyValuePair<string, object?>> entries,
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

    private static void WriteArray(
        BinaryWriter writer,
        object identity,
        int length,
        IEnumerable<(int Index, object? Value)> entries,
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
        writer.Write(buffer.InternalData);
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

    private static object? ReadValue(
        BinaryReader reader,
        DecodingState state,
        int depth)
    {
        if (depth > MaximumDepth)
            throw DataCloneError("Structured-clone depth exceeds the finite limit.");
        return reader.ReadByte() switch
        {
            0 => Undefined.value,
            1 => null,
            2 => false,
            3 => true,
            4 => reader.ReadDouble(),
            5 => ReadString(reader, state),
            6 => ReadObject(reader, state, depth),
            7 => ReadArray(reader, state, depth),
            8 => ReadBuffer(reader, state),
            9 => ReadReference(reader, state),
            _ => throw DataCloneError("Structured-clone payload contains an unknown value tag."),
        };
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

    private static string ReadString(BinaryReader reader, DecodingState state)
    {
        var length = reader.ReadInt32();
        if (length < 0)
            throw DataCloneError("Structured-clone string length is negative.");
        if (length > MaximumStringCodeUnits - state.StringCodeUnits)
            throw DataCloneError("Structured-clone string budget exceeds the finite limit.");
        state.StringCodeUnits += length;
        var characters = new char[length];
        for (var index = 0; index < length; index++)
            characters[index] = (char)reader.ReadUInt16();
        return new string(characters);
    }

    private static void WriteString(
        BinaryWriter writer,
        string value,
        EncodingState state)
    {
        if (value.Length > MaximumStringCodeUnits - state.StringCodeUnits)
            throw DataCloneError("Structured-clone string budget exceeds the finite limit.");
        state.StringCodeUnits += value.Length;
        writer.Write(value.Length);
        foreach (var character in value)
            writer.Write((ushort)character);
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
        while (value is TsValue typed) value = typed.unwrap();
        while (value is TsUnion union) value = union.unwrap();
        return value;
    }

    private static IEnumerable<(int Index, object? Value)> DynamicArrayEntries(
        IDynamicArray target)
    {
        for (var index = 0; index < target.Length; index++)
            if (target.HasIndex(index) && target.TryGetAt(index, out var value))
                yield return (index, value);
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
