using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously retrieves statistics for a file descriptor.
    /// </summary>
    /// <param name="fd">The file descriptor.</param>
    /// <returns>A Task that resolves to a Stats object.</returns>
    public static Task<Stats> fstat(int fd)
    {
        return Task.Run(() => fstatSync(fd));
    }
}
