using System;
using System.Buffers.Binary;

namespace Tsonic.CSharp.Node;

public partial class Buffer
{
    // 8-bit reads
    /// <summary>
    /// Reads an unsigned 8-bit integer from buf at the specified offset.
    /// </summary>
    public byte readUInt8(int offset = 0) => _data[offset];

    /// <summary>
    /// Reads an unsigned 8-bit integer from buf at the specified offset (alias).
    /// </summary>
    public byte readUint8(int offset = 0) => readUInt8(offset);

    /// <summary>
    /// Reads a signed 8-bit integer from buf at the specified offset.
    /// </summary>
    public sbyte readInt8(int offset = 0) => (sbyte)_data[offset];

    // 16-bit reads (Little Endian)
    /// <summary>
    /// Reads an unsigned 16-bit integer from buf at the specified offset with little-endian format.
    /// </summary>
    public ushort readUInt16LE(int offset = 0)
    {
        return BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(offset));
    }

    /// <summary>
    /// Reads an unsigned 16-bit integer from buf at the specified offset with little-endian format (alias).
    /// </summary>
    public ushort readUint16LE(int offset = 0) => readUInt16LE(offset);

    /// <summary>
    /// Reads a signed 16-bit integer from buf at the specified offset with little-endian format.
    /// </summary>
    public short readInt16LE(int offset = 0)
    {
        return BinaryPrimitives.ReadInt16LittleEndian(_data.AsSpan(offset));
    }

    // 16-bit reads (Big Endian)
    /// <summary>
    /// Reads an unsigned 16-bit integer from buf at the specified offset with big-endian format.
    /// </summary>
    public ushort readUInt16BE(int offset = 0)
    {
        return BinaryPrimitives.ReadUInt16BigEndian(_data.AsSpan(offset));
    }

    /// <summary>
    /// Reads an unsigned 16-bit integer from buf at the specified offset with big-endian format (alias).
    /// </summary>
    public ushort readUint16BE(int offset = 0) => readUInt16BE(offset);

    /// <summary>
    /// Reads a signed 16-bit integer from buf at the specified offset with big-endian format.
    /// </summary>
    public short readInt16BE(int offset = 0)
    {
        return BinaryPrimitives.ReadInt16BigEndian(_data.AsSpan(offset));
    }

    // 32-bit reads (Little Endian)
    /// <summary>
    /// Reads an unsigned 32-bit integer from buf at the specified offset with little-endian format.
    /// </summary>
    public uint readUInt32LE(int offset = 0)
    {
        return BinaryPrimitives.ReadUInt32LittleEndian(_data.AsSpan(offset));
    }

    /// <summary>
    /// Reads an unsigned 32-bit integer from buf at the specified offset with little-endian format (alias).
    /// </summary>
    public uint readUint32LE(int offset = 0) => readUInt32LE(offset);

    /// <summary>
    /// Reads a signed 32-bit integer from buf at the specified offset with little-endian format.
    /// </summary>
    public int readInt32LE(int offset = 0)
    {
        return BinaryPrimitives.ReadInt32LittleEndian(_data.AsSpan(offset));
    }

    // 32-bit reads (Big Endian)
    /// <summary>
    /// Reads an unsigned 32-bit integer from buf at the specified offset with big-endian format.
    /// </summary>
    public uint readUInt32BE(int offset = 0)
    {
        return BinaryPrimitives.ReadUInt32BigEndian(_data.AsSpan(offset));
    }

    /// <summary>
    /// Reads an unsigned 32-bit integer from buf at the specified offset with big-endian format (alias).
    /// </summary>
    public uint readUint32BE(int offset = 0) => readUInt32BE(offset);

    /// <summary>
    /// Reads a signed 32-bit integer from buf at the specified offset with big-endian format.
    /// </summary>
    public int readInt32BE(int offset = 0)
    {
        return BinaryPrimitives.ReadInt32BigEndian(_data.AsSpan(offset));
    }

    // 64-bit reads (Little Endian)
    /// <summary>
    /// Reads an unsigned 64-bit integer from buf at the specified offset with little-endian format.
    /// </summary>
    public ulong readBigUInt64LE(int offset = 0)
    {
        return BinaryPrimitives.ReadUInt64LittleEndian(_data.AsSpan(offset));
    }

    /// <summary>
    /// Reads an unsigned 64-bit integer from buf at the specified offset with little-endian format (alias).
    /// </summary>
    public ulong readBigUint64LE(int offset = 0) => readBigUInt64LE(offset);

    /// <summary>
    /// Reads a signed 64-bit integer from buf at the specified offset with little-endian format.
    /// </summary>
    public long readBigInt64LE(int offset = 0)
    {
        return BinaryPrimitives.ReadInt64LittleEndian(_data.AsSpan(offset));
    }

    // 64-bit reads (Big Endian)
    /// <summary>
    /// Reads an unsigned 64-bit integer from buf at the specified offset with big-endian format.
    /// </summary>
    public ulong readBigUInt64BE(int offset = 0)
    {
        return BinaryPrimitives.ReadUInt64BigEndian(_data.AsSpan(offset));
    }

    /// <summary>
    /// Reads an unsigned 64-bit integer from buf at the specified offset with big-endian format (alias).
    /// </summary>
    public ulong readBigUint64BE(int offset = 0) => readBigUInt64BE(offset);

    /// <summary>
    /// Reads a signed 64-bit integer from buf at the specified offset with big-endian format.
    /// </summary>
    public long readBigInt64BE(int offset = 0)
    {
        return BinaryPrimitives.ReadInt64BigEndian(_data.AsSpan(offset));
    }

    // Floating point reads (Little Endian)
    /// <summary>
    /// Reads a 32-bit float from buf at the specified offset with little-endian format.
    /// </summary>
    public float readFloatLE(int offset = 0)
    {
        return BitConverter.Int32BitsToSingle(readInt32LE(offset));
    }

    /// <summary>
    /// Reads a 64-bit double from buf at the specified offset with little-endian format.
    /// </summary>
    public double readDoubleLE(int offset = 0)
    {
        return BitConverter.Int64BitsToDouble(readBigInt64LE(offset));
    }

    // Floating point reads (Big Endian)
    /// <summary>
    /// Reads a 32-bit float from buf at the specified offset with big-endian format.
    /// </summary>
    public float readFloatBE(int offset = 0)
    {
        return BitConverter.Int32BitsToSingle(readInt32BE(offset));
    }

    /// <summary>
    /// Reads a 64-bit double from buf at the specified offset with big-endian format.
    /// </summary>
    public double readDoubleBE(int offset = 0)
    {
        return BitConverter.Int64BitsToDouble(readBigInt64BE(offset));
    }

    // Variable byte-length reads (Little Endian)
    /// <summary>
    /// Reads byteLength number of bytes from buf at the specified offset and interprets the result as an unsigned integer supporting up to 48 bits of accuracy.
    /// </summary>
    public ulong readUIntLE(int offset, int byteLength)
    {
        if (byteLength < 1 || byteLength > 6)
            throw new ArgumentException("byteLength must be between 1 and 6");

        ulong value = 0;
        for (int i = 0; i < byteLength; i++)
        {
            value |= ((ulong)_data[offset + i]) << (i * 8);
        }
        return value;
    }

    /// <summary>
    /// Reads byteLength number of bytes from buf at the specified offset and interprets the result as an unsigned integer (alias).
    /// </summary>
    public ulong readUintLE(int offset, int byteLength) => readUIntLE(offset, byteLength);

    /// <summary>
    /// Reads byteLength number of bytes from buf at the specified offset and interprets the result as a two's complement signed value supporting up to 48 bits of accuracy.
    /// </summary>
    public long readIntLE(int offset, int byteLength)
    {
        if (byteLength < 1 || byteLength > 6)
            throw new ArgumentException("byteLength must be between 1 and 6");

        long value = 0;
        for (int i = 0; i < byteLength; i++)
        {
            value |= ((long)_data[offset + i]) << (i * 8);
        }

        // Sign extend
        var bits = byteLength * 8;
        if ((value & (1L << (bits - 1))) != 0)
        {
            value |= (-1L << bits);
        }

        return value;
    }

    // Variable byte-length reads (Big Endian)
    /// <summary>
    /// Reads byteLength number of bytes from buf at the specified offset and interprets the result as an unsigned integer supporting up to 48 bits of accuracy.
    /// </summary>
    public ulong readUIntBE(int offset, int byteLength)
    {
        if (byteLength < 1 || byteLength > 6)
            throw new ArgumentException("byteLength must be between 1 and 6");

        ulong value = 0;
        for (int i = 0; i < byteLength; i++)
        {
            value = (value << 8) | _data[offset + i];
        }
        return value;
    }

    /// <summary>
    /// Reads byteLength number of bytes from buf at the specified offset and interprets the result as an unsigned integer (alias).
    /// </summary>
    public ulong readUintBE(int offset, int byteLength) => readUIntBE(offset, byteLength);

    /// <summary>
    /// Reads byteLength number of bytes from buf at the specified offset and interprets the result as a two's complement signed value supporting up to 48 bits of accuracy.
    /// </summary>
    public long readIntBE(int offset, int byteLength)
    {
        if (byteLength < 1 || byteLength > 6)
            throw new ArgumentException("byteLength must be between 1 and 6");

        long value = 0;
        for (int i = 0; i < byteLength; i++)
        {
            value = (value << 8) | _data[offset + i];
        }

        // Sign extend
        var bits = byteLength * 8;
        if ((value & (1L << (bits - 1))) != 0)
        {
            value |= (-1L << bits);
        }

        return value;
    }
}
