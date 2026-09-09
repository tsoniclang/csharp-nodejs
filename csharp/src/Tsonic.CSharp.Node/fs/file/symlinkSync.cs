namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously creates a symbolic link.
    /// </summary>
    /// <param name="target">The target path.</param>
    /// <param name="path">The symbolic link path.</param>
    /// <param name="type">The type of symbolic link (file or directory).</param>
    public static void symlinkSync(string target, string path, string? type = null)
    {
        // Determine if target is a directory
        var isDirectory = Directory.Exists(target) || type == "dir" || type == "junction";

        if (isDirectory)
        {
            Directory.CreateSymbolicLink(path, target);
        }
        else
        {
            File.CreateSymbolicLink(path, target);
        }
    }
}
