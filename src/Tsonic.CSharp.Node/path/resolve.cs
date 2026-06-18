using System.Linq;

namespace Tsonic.CSharp.Node;

public static partial class path
{
    /// <summary>
    /// Resolves a sequence of paths or path segments into an absolute path.
    /// </summary>
    /// <param name="paths">A sequence of paths or path segments.</param>
    /// <returns>The resolved absolute path.</returns>
    public static string resolve(params string[] paths)
    {
        if (paths.Length == 0)
            return Directory.GetCurrentDirectory();

        var validPaths = paths.Where(p => !string.IsNullOrEmpty(p)).ToArray();

        if (validPaths.Length == 0)
            return Directory.GetCurrentDirectory();

        // Start from current directory and resolve each path segment
        string result = Directory.GetCurrentDirectory();

        foreach (var p in validPaths)
        {
            if (Path.IsPathRooted(p))
            {
                result = p;
            }
            else
            {
                result = Path.Combine(result, p);
            }
        }

        return Path.GetFullPath(result);
    }
}
