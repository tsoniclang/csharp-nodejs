namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously renames (moves) a file or directory.
    /// </summary>
    /// <param name="oldPath">The old path.</param>
    /// <param name="newPath">The new path.</param>
    public static void renameSync(string oldPath, string newPath)
    {
        if (File.Exists(oldPath))
        {
            File.Move(oldPath, newPath);
        }
        else if (Directory.Exists(oldPath))
        {
            Directory.Move(oldPath, newPath);
        }
        else
        {
            throw new FileNotFoundException($"No such file or directory: {oldPath}");
        }
    }
}
