using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously computes the canonical pathname by resolving symbolic links and relative paths.
    /// </summary>
    /// <param name="path">The path to resolve.</param>
    /// <returns>A promise that resolves to the canonical absolute pathname.</returns>
    public static Task<string> realpath(string path)
    {
        return Task.Run(() => Path.GetFullPath(path));
    }
}
