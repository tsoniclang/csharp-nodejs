namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously reads the contents of a directory.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <returns>An array of filenames.</returns>
    public static string[] readdirSync(string path)
    {
        return Directory.GetFileSystemEntries(path)
            .Select(Path.GetFileName)
            .Where(name => !string.IsNullOrEmpty(name))
            .Select(name => name!)
            .ToArray();
    }

    /// <summary>
    /// Synchronously reads the contents of a directory.
    /// </summary>
    /// <param name="path">The directory path.</param>
    /// <param name="withFileTypes">If true, returns directory entries with type info.</param>
    /// <returns>An array of filenames or directory entries.</returns>
    public static object[] readdirSync(string path, bool withFileTypes)
    {
        return withFileTypes
            ? readdirDirentsSync(path).Cast<object>().ToArray()
            : readdirSync(path).Cast<object>().ToArray();
    }
}
