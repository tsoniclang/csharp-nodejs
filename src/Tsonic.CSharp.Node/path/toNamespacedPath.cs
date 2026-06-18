using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node;

public static partial class path
{
    /// <summary>
    /// Returns an equivalent namespace-prefixed path (Windows only).
    /// On POSIX systems, returns the path unchanged.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The namespace-prefixed path on Windows, or the original path on POSIX.</returns>
    public static string toNamespacedPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // On POSIX, return path unchanged
            return path;
        }

        // On Windows, prefix with \\?\ if not already prefixed
        var fullPath = Path.GetFullPath(path);

        if (fullPath.StartsWith(@"\\?\") || fullPath.StartsWith(@"\\.\"))
        {
            return fullPath;
        }

        if (fullPath.StartsWith(@"\\"))
        {
            // UNC path: \\server\share => \\?\UNC\server\share
            return @"\\?\UNC\" + fullPath.Substring(2);
        }

        // Regular path: C:\path => \\?\C:\path
        return @"\\?\" + fullPath;
    }
}
