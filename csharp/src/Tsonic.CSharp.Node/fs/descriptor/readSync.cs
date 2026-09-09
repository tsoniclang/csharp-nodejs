using System;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously reads data from a file descriptor into a buffer.
    /// </summary>
    /// <param name="fd">The file descriptor.</param>
    /// <param name="buffer">The buffer to write data into.</param>
    /// <param name="offset">The position in the buffer to start writing to.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <param name="position">The position in the file to start reading from. If null, reads from current position.</param>
    /// <returns>The number of bytes read.</returns>
    public static int readSync(int fd, byte[] buffer, int offset, int length, int? position)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        var stream = FileDescriptorManager.Get(fd);
        if (stream == null)
            throw new ArgumentException($"Bad file descriptor: {fd}", nameof(fd));

        if (offset < 0 || offset >= buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));

        if (length < 0 || offset + length > buffer.Length)
            throw new ArgumentOutOfRangeException(nameof(length));

        // Set position if specified
        if (position.HasValue && stream.CanSeek)
        {
            stream.Position = position.Value;
        }

        return stream.Read(buffer, offset, length);
    }

    /// <summary>
    /// Synchronously reads data from a file descriptor into a Buffer.
    /// </summary>
    /// <param name="fd">The file descriptor.</param>
    /// <param name="buffer">The Buffer to write data into.</param>
    /// <param name="offset">The position in the Buffer to start writing to.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <param name="position">The position in the file to start reading from. If null, reads from current position.</param>
    /// <returns>The number of bytes read.</returns>
    public static int readSync(int fd, Buffer buffer, int offset, int length, int? position)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        return readSync(fd, buffer.InternalData, offset, length, position);
    }
}
