using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously changes the permissions of a file.
    /// </summary>
    /// <param name="path">The file path.</param>
    /// <param name="mode">The permission mode (Unix-style permissions).</param>
    /// <returns>A promise that resolves when the permissions are changed.</returns>
    public static Task chmod(string path, int mode)
    {
        return Task.Run(() => chmodSync(path, mode));
    }
}
