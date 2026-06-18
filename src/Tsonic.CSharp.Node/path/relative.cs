namespace Tsonic.CSharp.Node;

public static partial class path
{
    /// <summary>
    /// Returns the relative path from 'from' to 'to' based on the current working directory.
    /// </summary>
    /// <param name="from">The source path.</param>
    /// <param name="to">The destination path.</param>
    /// <returns>The relative path.</returns>
    public static string relative(string from, string to)
    {
        if (string.IsNullOrEmpty(from))
            from = Directory.GetCurrentDirectory();
        if (string.IsNullOrEmpty(to))
            to = Directory.GetCurrentDirectory();

        // Get absolute paths
        var fromPath = Path.GetFullPath(from);
        var toPath = Path.GetFullPath(to);

        if (fromPath == toPath)
            return string.Empty;

        return Path.GetRelativePath(fromPath, toPath);
    }
}
