using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously removes a directory.
    /// </summary>
    /// <param name="path">The directory path to remove.</param>
    /// <param name="recursive">If true, removes directory and all contents (default: false).</param>
    /// <returns>A promise that resolves when the directory is removed.</returns>
    public static Task rmdir(string path, bool recursive = false)
    {
        return Task.Run(() => Directory.Delete(path, recursive));
    }
}
