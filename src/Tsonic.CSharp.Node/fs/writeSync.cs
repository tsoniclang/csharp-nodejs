using System;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously writes data from a buffer to a file descriptor.
    /// </summary>
    /// <param name="fd">The file descriptor.</param>
    /// <param name="buffer">The buffer to read data from.</param>
    /// <param name="offset">The position in the buffer to start reading from.</param>
    /// <param name="length">The number of bytes to write.</param>
    /// <param name="position">The position in the file to start writing to. If null, writes at current position.</param>
    /// <returns>The number of bytes written.</returns>
    public static int writeSync(int fd, byte[] buffer, int offset, int length, int? position)
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

        stream.Write(buffer, offset, length);
        return length;
    }

    /// <summary>
    /// Synchronously writes data from a Buffer to a file descriptor.
    /// </summary>
    /// <param name="fd">The file descriptor.</param>
    /// <param name="buffer">The Buffer to read data from.</param>
    /// <param name="offset">The position in the Buffer to start reading from.</param>
    /// <param name="length">The number of bytes to write.</param>
    /// <param name="position">The position in the file to start writing to. If null, writes at current position.</param>
    /// <returns>The number of bytes written.</returns>
    public static int writeSync(int fd, Buffer buffer, int offset, int length, int? position)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        return writeSync(fd, buffer.InternalData, offset, length, position);
    }

    /// <summary>
    /// Synchronously writes a string to a file descriptor.
    /// </summary>
    /// <param name="fd">The file descriptor.</param>
    /// <param name="data">The string data to write.</param>
    /// <param name="position">The position in the file to start writing to. If null, writes at current position.</param>
    /// <param name="encoding">The string encoding. Default is "utf8".</param>
    /// <returns>The number of bytes written.</returns>
    public static int writeSync(int fd, string data, int? position = null, string? encoding = null)
    {
        var enc = System.Text.Encoding.UTF8; // Default encoding
        if (encoding != null)
        {
            enc = encoding.ToLowerInvariant() switch
            {
                "utf8" or "utf-8" => System.Text.Encoding.UTF8,
                "ascii" => System.Text.Encoding.ASCII,
                "utf16le" or "utf-16le" or "ucs2" or "ucs-2" => System.Text.Encoding.Unicode,
                "latin1" or "binary" => System.Text.Encoding.Latin1,
                _ => System.Text.Encoding.UTF8
            };
        }

        var bytes = enc.GetBytes(data);
        return writeSync(fd, bytes, 0, bytes.Length, position);
    }
}
