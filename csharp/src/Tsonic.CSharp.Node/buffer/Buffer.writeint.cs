using System.Buffers.Binary;

namespace Tsonic.CSharp.Node;

public partial class Buffer
{
    // 8-bit writes
    /// <summary>
    /// Writes value to buf at the specified offset. value must be a valid unsigned 8-bit integer.
    /// </summary>
    public int writeUInt8(byte value, int offset = 0)
    {
        _data[offset] = value;
        return offset + 1;
    }

    /// <summary>
    /// Writes value to buf at the specified offset (alias).
    /// </summary>
    public int writeUint8(byte value, int offset = 0) => writeUInt8(value, offset);

    /// <summary>
    /// Writes value to buf at the specified offset. value must be a valid signed 8-bit integer.
    /// </summary>
    public int writeInt8(sbyte value, int offset = 0)
    {
        _data[offset] = (byte)value;
        return offset + 1;
    }

    // 16-bit writes (Little Endian)
    /// <summary>
    /// Writes value to buf at the specified offset with little-endian format.
    /// </summary>
    public int writeUInt16LE(ushort value, int offset = 0)
    {
        BinaryPrimitives.WriteUInt16LittleEndian(_data.AsSpan(offset), value);
        return offset + 2;
    }

    /// <summary>
    /// Writes value to buf at the specified offset with little-endian format (alias).
    /// </summary>
    public int writeUint16LE(ushort value, int offset = 0) => writeUInt16LE(value, offset);

    /// <summary>
    /// Writes value to buf at the specified offset with little-endian format.
    /// </summary>
    public int writeInt16LE(short value, int offset = 0)
    {
        BinaryPrimitives.WriteInt16LittleEndian(_data.AsSpan(offset), value);
        return offset + 2;
    }

    // 16-bit writes (Big Endian)
    /// <summary>
    /// Writes value to buf at the specified offset with big-endian format.
    /// </summary>
    public int writeUInt16BE(ushort value, int offset = 0)
    {
        BinaryPrimitives.WriteUInt16BigEndian(_data.AsSpan(offset), value);
        return offset + 2;
    }

    /// <summary>
    /// Writes value to buf at the specified offset with big-endian format (alias).
    /// </summary>
    public int writeUint16BE(ushort value, int offset = 0) => writeUInt16BE(value, offset);

    /// <summary>
    /// Writes value to buf at the specified offset with big-endian format.
    /// </summary>
    public int writeInt16BE(short value, int offset = 0)
    {
        BinaryPrimitives.WriteInt16BigEndian(_data.AsSpan(offset), value);
        return offset + 2;
    }

    // 32-bit writes (Little Endian)
    /// <summary>
    /// Writes value to buf at the specified offset with little-endian format.
    /// </summary>
    public int writeUInt32LE(uint value, int offset = 0)
    {
        BinaryPrimitives.WriteUInt32LittleEndian(_data.AsSpan(offset), value);
        return offset + 4;
    }

    /// <summary>
    /// Writes value to buf at the specified offset with little-endian format (alias).
    /// </summary>
    public int writeUint32LE(uint value, int offset = 0) => writeUInt32LE(value, offset);

    /// <summary>
    /// Writes value to buf at the specified offset with little-endian format.
    /// </summary>
    public int writeInt32LE(int value, int offset = 0)
    {
        BinaryPrimitives.WriteInt32LittleEndian(_data.AsSpan(offset), value);
        return offset + 4;
    }

    // 32-bit writes (Big Endian)
    /// <summary>
    /// Writes value to buf at the specified offset with big-endian format.
    /// </summary>
    public int writeUInt32BE(uint value, int offset = 0)
    {
        BinaryPrimitives.WriteUInt32BigEndian(_data.AsSpan(offset), value);
        return offset + 4;
    }

    /// <summary>
    /// Writes value to buf at the specified offset with big-endian format (alias).
    /// </summary>
    public int writeUint32BE(uint value, int offset = 0) => writeUInt32BE(value, offset);

    /// <summary>
    /// Writes value to buf at the specified offset with big-endian format.
    /// </summary>
    public int writeInt32BE(int value, int offset = 0)
    {
        BinaryPrimitives.WriteInt32BigEndian(_data.AsSpan(offset), value);
        return offset + 4;
    }

    // 64-bit writes (Little Endian)
    /// <summary>
    /// Writes value to buf at the specified offset with little-endian format.
    /// </summary>
    public int writeBigUInt64LE(ulong value, int offset = 0)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(_data.AsSpan(offset), value);
        return offset + 8;
    }

    /// <summary>
    /// Writes value to buf at the specified offset with little-endian format (alias).
    /// </summary>
    public int writeBigUint64LE(ulong value, int offset = 0) => writeBigUInt64LE(value, offset);

    /// <summary>
    /// Writes value to buf at the specified offset with little-endian format.
    /// </summary>
    public int writeBigInt64LE(long value, int offset = 0)
    {
        BinaryPrimitives.WriteInt64LittleEndian(_data.AsSpan(offset), value);
        return offset + 8;
    }

    // 64-bit writes (Big Endian)
    /// <summary>
    /// Writes value to buf at the specified offset with big-endian format.
    /// </summary>
    public int writeBigUInt64BE(ulong value, int offset = 0)
    {
        BinaryPrimitives.WriteUInt64BigEndian(_data.AsSpan(offset), value);
        return offset + 8;
    }

    /// <summary>
    /// Writes value to buf at the specified offset with big-endian format (alias).
    /// </summary>
    public int writeBigUint64BE(ulong value, int offset = 0) => writeBigUInt64BE(value, offset);

    /// <summary>
    /// Writes value to buf at the specified offset with big-endian format.
    /// </summary>
    public int writeBigInt64BE(long value, int offset = 0)
    {
        BinaryPrimitives.WriteInt64BigEndian(_data.AsSpan(offset), value);
        return offset + 8;
    }

    // Floating point writes (Little Endian)
    /// <summary>
    /// Writes value to buf at the specified offset with little-endian format.
    /// </summary>
    public int writeFloatLE(float value, int offset = 0)
    {
        return writeInt32LE(System.BitConverter.SingleToInt32Bits(value), offset);
    }

    /// <summary>
    /// Writes value to buf at the specified offset with little-endian format.
    /// </summary>
    public int writeDoubleLE(double value, int offset = 0)
    {
        return writeBigInt64LE(System.BitConverter.DoubleToInt64Bits(value), offset);
    }

    // Floating point writes (Big Endian)
    /// <summary>
    /// Writes value to buf at the specified offset with big-endian format.
    /// </summary>
    public int writeFloatBE(float value, int offset = 0)
    {
        return writeInt32BE(System.BitConverter.SingleToInt32Bits(value), offset);
    }

    /// <summary>
    /// Writes value to buf at the specified offset with big-endian format.
    /// </summary>
    public int writeDoubleBE(double value, int offset = 0)
    {
        return writeBigInt64BE(System.BitConverter.DoubleToInt64Bits(value), offset);
    }

    // Variable byte-length writes (Little Endian)
    /// <summary>
    /// Writes byteLength bytes of value to buf at the specified offset with little-endian format.
    /// </summary>
    public int writeUIntLE(ulong value, int offset, int byteLength)
    {
        if (byteLength < 1 || byteLength > 6)
            throw new System.ArgumentException("byteLength must be between 1 and 6");

        for (int i = 0; i < byteLength; i++)
        {
            _data[offset + i] = (byte)(value & 0xFF);
            value >>= 8;
        }

        return offset + byteLength;
    }

    /// <summary>
    /// Writes byteLength bytes of value to buf at the specified offset with little-endian format (alias).
    /// </summary>
    public int writeUintLE(ulong value, int offset, int byteLength) => writeUIntLE(value, offset, byteLength);

    /// <summary>
    /// Writes byteLength bytes of value to buf at the specified offset with little-endian format.
    /// </summary>
    public int writeIntLE(long value, int offset, int byteLength)
    {
        if (byteLength < 1 || byteLength > 6)
            throw new System.ArgumentException("byteLength must be between 1 and 6");

        for (int i = 0; i < byteLength; i++)
        {
            _data[offset + i] = (byte)(value & 0xFF);
            value >>= 8;
        }

        return offset + byteLength;
    }

    // Variable byte-length writes (Big Endian)
    /// <summary>
    /// Writes byteLength bytes of value to buf at the specified offset with big-endian format.
    /// </summary>
    public int writeUIntBE(ulong value, int offset, int byteLength)
    {
        if (byteLength < 1 || byteLength > 6)
            throw new System.ArgumentException("byteLength must be between 1 and 6");

        for (int i = byteLength - 1; i >= 0; i--)
        {
            _data[offset + i] = (byte)(value & 0xFF);
            value >>= 8;
        }

        return offset + byteLength;
    }

    /// <summary>
    /// Writes byteLength bytes of value to buf at the specified offset with big-endian format (alias).
    /// </summary>
    public int writeUintBE(ulong value, int offset, int byteLength) => writeUIntBE(value, offset, byteLength);

    /// <summary>
    /// Writes byteLength bytes of value to buf at the specified offset with big-endian format.
    /// </summary>
    public int writeIntBE(long value, int offset, int byteLength)
    {
        if (byteLength < 1 || byteLength > 6)
            throw new System.ArgumentException("byteLength must be between 1 and 6");

        for (int i = byteLength - 1; i >= 0; i--)
        {
            _data[offset + i] = (byte)(value & 0xFF);
            value >>= 8;
        }

        return offset + byteLength;
    }
}
