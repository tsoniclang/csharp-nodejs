using System.Numerics;

namespace Tsonic.CSharp.Node;

internal static partial class StructuredClone
{
    private static BigInteger ReadBigInteger(BinaryReader reader, DecodingState state)
    {
        var length = reader.ReadInt32();
        if (length <= 0 || length > WorkerTransport.MaximumFrameBytes)
            throw DataCloneError("Structured-clone integer length exceeds the finite limit.");
        Reserve(length, state);
        var bytes = reader.ReadBytes(length);
        if (bytes.Length != length) throw new EndOfStreamException("Structured-clone integer is truncated.");
        return new BigInteger(bytes);
    }

    private static void WriteWideInteger(BinaryWriter writer, UInt128 value)
    {
        writer.Write(unchecked((ulong)value));
        writer.Write((ulong)(value >> 64));
    }

    private static UInt128 ReadWideInteger(BinaryReader reader)
    {
        var low = reader.ReadUInt64();
        var high = reader.ReadUInt64();
        return ((UInt128)high << 64) | low;
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
}
