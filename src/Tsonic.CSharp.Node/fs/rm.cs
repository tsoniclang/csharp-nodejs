using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously removes files and directories (modern API).
    /// </summary>
    /// <param name="path">The path to remove.</param>
    /// <param name="recursive">If true, removes directories and contents recursively.</param>
    /// <returns>A promise that resolves when the removal is complete.</returns>
    public static Task rm(string path, bool recursive = false)
    {
        return Task.Run(() => rmSync(path, recursive));
    }
}
