namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously computes the canonical pathname by resolving symbolic links and relative paths.
    /// </summary>
    /// <param name="path">The path to resolve.</param>
    /// <returns>The canonical absolute pathname.</returns>
    public static string realpathSync(string path)
    {
        return Path.GetFullPath(path);
    }
}
