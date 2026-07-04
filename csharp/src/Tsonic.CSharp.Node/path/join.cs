using System.Linq;

namespace Tsonic.CSharp.Node;

public static partial class path
{
    /// <summary>
    /// Joins all given path segments together using the platform-specific separator, then normalizes the resulting path.
    /// </summary>
    /// <param name="paths">A sequence of path segments.</param>
    /// <returns>The joined path.</returns>
    public static string join(params string[] paths)
    {
        if (paths.Length == 0)
            return ".";

        // Filter out empty strings like Node.js does
        var validPaths = paths.Where(p => !string.IsNullOrEmpty(p)).ToArray();

        if (validPaths.Length == 0)
            return ".";

        return Path.Combine(validPaths);
    }
}
