using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously reads the contents of a symbolic link.
    /// </summary>
    /// <param name="path">The symbolic link path.</param>
    /// <returns>A promise that resolves to the target of the symbolic link.</returns>
    public static Task<string> readlink(string path)
    {
        return Task.Run(() => readlinkSync(path));
    }
}
