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
    public static Task<string[]> readdir(string path, bool withFileTypes = false)
    {
        return Task.Run<string[]>(() =>
        {
            if (withFileTypes)
            {
                throw new NotSupportedException("withFileTypes option not yet implemented.");
            }

            return Directory.GetFileSystemEntries(path)
                .Select(Path.GetFileName)
                .Where(name => !string.IsNullOrEmpty(name))
                .ToArray()!;
        });
    }
}
