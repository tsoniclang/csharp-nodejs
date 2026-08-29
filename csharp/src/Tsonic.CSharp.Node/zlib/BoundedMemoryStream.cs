using System;
using System.IO;

namespace Tsonic.CSharp.Node;

internal sealed class BoundedMemoryStream : MemoryStream
{
    private readonly long _maximumLength;

    public BoundedMemoryStream(int? maximumLength)
    {
        if (maximumLength is <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumLength));
        _maximumLength = maximumLength ?? int.MaxValue;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        EnsureCapacityFor(count);
        base.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        EnsureCapacityFor(buffer.Length);
        base.Write(buffer);
    }

    public override void WriteByte(byte value)
    {
        EnsureCapacityFor(1);
        base.WriteByte(value);
    }

    private void EnsureCapacityFor(int count)
    {
        if (checked(Position + count) > _maximumLength)
            throw new InvalidDataException("Codec output exceeds maxOutputLength.");
    }
}
