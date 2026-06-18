using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously closes a file descriptor.
    /// </summary>
    /// <param name="fd">The file descriptor.</param>
    /// <returns>A Task that completes when the file is closed.</returns>
    public static Task close(int fd)
    {
        return Task.Run(() => closeSync(fd));
    }
}
