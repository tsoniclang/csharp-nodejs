namespace Tsonic.CSharp.Node;

public static partial class path
{
    /// <summary>
    /// Normalizes the given path, resolving '..' and '.' segments.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The normalized path.</returns>
    public static string normalize(string path)
    {
        if (string.IsNullOrEmpty(path))
            return ".";

        return Path.GetFullPath(path);
    }
}
