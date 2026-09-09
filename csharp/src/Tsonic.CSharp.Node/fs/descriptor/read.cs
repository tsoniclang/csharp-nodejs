using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously reads data from a file descriptor into a buffer.
    /// </summary>
    /// <param name="fd">The file descriptor.</param>
    /// <param name="buffer">The buffer to write data into.</param>
    /// <param name="offset">The position in the buffer to start writing to.</param>
    /// <param name="length">The number of bytes to read.</param>
    /// <param name="position">The position in the file to start reading from. If null, reads from current position.</param>
    /// <returns>A Task that resolves to the number of bytes read.</returns>
    public static Task<int> read(int fd, byte[] buffer, int offset, int length, int? position)
    {
        return Task.Run(() => readSync(fd, buffer, offset, length, position));
    }
}
