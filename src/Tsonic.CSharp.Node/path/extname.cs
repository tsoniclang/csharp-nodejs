namespace Tsonic.CSharp.Node;

public static partial class path
{
    /// <summary>
    /// Returns the extension of the path, from the last occurrence of the . (period) character to end of string.
    /// </summary>
    /// <param name="path">The path to process.</param>
    /// <returns>The extension (including the period), or empty string if no extension.</returns>
    public static string extname(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        return Path.GetExtension(path);
    }
}
