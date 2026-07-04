using System;

namespace Tsonic.CSharp.Node;

public partial class Buffer
{
    /// <summary>
    /// Allocates a new Buffer of size bytes. If fill is undefined, the Buffer will be zero-filled.
    /// </summary>
    /// <param name="size">The desired length of the new Buffer.</param>
    /// <param name="fill">A value to pre-fill the new Buffer with.</param>
    /// <param name="encoding">If fill is a string, this is its encoding.</param>
    /// <returns>A new Buffer instance.</returns>
    public static Buffer alloc(int size, object? fill = null, string? encoding = null)
    {
        if (size < 0)
            throw new ArgumentException("Size must be non-negative", nameof(size));

        var buffer = new Buffer(new byte[size]);

        if (fill != null)
        {
            if (fill is string str)
            {
                buffer.fill(str, 0, size, encoding ?? "utf8");
            }
            else if (fill is int intValue)
            {
                buffer.fill(intValue, 0, size);
            }
            else if (fill is Buffer bufferValue)
            {
                buffer.fill(bufferValue, 0, size);
            }
        }

        return buffer;
    }

    /// <summary>
    /// Allocates a new Buffer of size bytes without initializing the memory.
    /// The contents are unknown and may contain old/sensitive data.
    /// </summary>
    /// <param name="size">The desired length of the new Buffer.</param>
    /// <returns>A new Buffer instance.</returns>
    public static Buffer allocUnsafe(int size)
    {
        if (size < 0)
            throw new ArgumentException("Size must be non-negative", nameof(size));

        return new Buffer(new byte[size]);
    }

    /// <summary>
    /// Allocates a new non-pooled Buffer of size bytes.
    /// </summary>
    /// <param name="size">The desired length of the new Buffer.</param>
    /// <returns>A new Buffer instance.</returns>
    public static Buffer allocUnsafeSlow(int size)
    {
        if (size < 0)
            throw new ArgumentException("Size must be non-negative", nameof(size));

        return new Buffer(new byte[size]);
    }
}
