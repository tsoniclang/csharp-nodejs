namespace Tsonic.CSharp.Node;

public static partial class path
{
    /// <summary>
    /// Determines if path is an absolute path.
    /// </summary>
    /// <param name="path">The path to test.</param>
    /// <returns>True if the path is absolute, false otherwise.</returns>
    public static bool isAbsolute(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        return Path.IsPathRooted(path);
    }
}
