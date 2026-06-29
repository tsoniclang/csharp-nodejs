using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Asynchronously reads the contents of a directory.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <param name="withFileTypes">If true, returns directory entries with type info.</param>
    /// <returns>A promise that resolves to an array of filenames or directory entries.</returns>
    public static Task<object[]> readdir(string path, bool withFileTypes = false)
    {
        return Task.Run(() =>
        {
            return readdirSync(path, withFileTypes);
        });
    }

    /// <summary>
    /// Asynchronously reads directory entries with Node.js Dirent-style type information.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <returns>A promise that resolves to directory entries.</returns>
    public static Task<Dirent[]> readdirDirents(string path)
    {
        return Task.Run(() => readdirDirentsSync(path));
    }
}
