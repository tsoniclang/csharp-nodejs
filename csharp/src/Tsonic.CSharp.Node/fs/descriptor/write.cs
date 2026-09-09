using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously writes data from a buffer to a file descriptor.
    /// </summary>
    /// <param name="fd">The file descriptor.</param>
    /// <param name="buffer">The buffer to read data from.</param>
    /// <param name="offset">The position in the buffer to start reading from.</param>
    /// <param name="length">The number of bytes to write.</param>
    /// <param name="position">The position in the file to start writing to. If null, writes at current position.</param>
    /// <returns>A Task that resolves to the number of bytes written.</returns>
    public static Task<int> write(int fd, byte[] buffer, int offset, int length, int? position)
    {
        return Task.Run(() => writeSync(fd, buffer, offset, length, position));
    }

    /// <summary>
    /// Asynchronously writes a string to a file descriptor.
    /// </summary>
    /// <param name="fd">The file descriptor.</param>
    /// <param name="data">The string data to write.</param>
    /// <param name="position">The position in the file to start writing to. If null, writes at current position.</param>
    /// <param name="encoding">The string encoding. Default is "utf8".</param>
    /// <returns>A Task that resolves to the number of bytes written.</returns>
    public static Task<int> write(int fd, string data, int? position = null, string? encoding = null)
    {
        return Task.Run(() => writeSync(fd, data, position, encoding));
    }
}
