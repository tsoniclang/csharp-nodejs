namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously reads the contents of a directory.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <param name="withFileTypes">If true, returns directory entries with type info.</param>
    /// <returns>An array of filenames or directory entries.</returns>
    public static string[] readdirSync(string path, bool withFileTypes = false)
    {
        if (withFileTypes)
        {
            throw new NotSupportedException("withFileTypes option not yet implemented.");
        }

        return Directory.GetFileSystemEntries(path)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrEmpty(name))
            .ToArray()!;
    }
}
