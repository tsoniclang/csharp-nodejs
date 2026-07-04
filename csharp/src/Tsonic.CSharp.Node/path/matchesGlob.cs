namespace Tsonic.CSharp.Node;

public static partial class path
{
    /// <summary>
    /// Determines if path matches the glob pattern.
    /// </summary>
    /// <param name="path">The path to glob-match against.</param>
    /// <param name="pattern">The glob pattern to check against.</param>
    /// <returns>True if the path matches the pattern.</returns>
    public static bool matchesGlob(string path, string pattern)
    {
        // Basic glob matching implementation
        // This is a simplified version - a full implementation would use proper glob parsing
        if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(pattern))
            return false;

        // Convert glob pattern to regex
        var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
            .Replace("\\*\\*", ".*")  // ** matches any number of path segments
            .Replace("\\*", "[^/\\\\]*")  // * matches within a path segment
            .Replace("\\?", ".")  // ? matches single character
            + "$";

        return System.Text.RegularExpressions.Regex.IsMatch(path, regexPattern);
    }
}
