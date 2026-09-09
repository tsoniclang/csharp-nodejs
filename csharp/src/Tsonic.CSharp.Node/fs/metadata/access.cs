using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously tests a user's permissions for the file or directory specified by path.
    /// </summary>
    /// <param name="path">The path to check.</param>
    /// <param name="mode">Optional mode (not fully implemented on all platforms).</param>
    /// <returns>A promise that resolves when the check is complete.</returns>
    public static Task access(string path, int mode = 0)
    {
        return Task.Run(() => accessSync(path, mode));
    }
}
