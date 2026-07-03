namespace Tsonic.CSharp.Node;

public static partial class path
{
    /// <summary>
    /// Returns the last portion of a path, similar to the Unix basename command.
    /// </summary>
    /// <param name="path">The path to process.</param>
    /// <param name="suffix">Optional suffix to remove.</param>
    /// <returns>The base name of the path.</returns>
    public static string basename(string path, string? suffix = null)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        var name = Path.GetFileName(path);

        if (suffix != null && name.EndsWith(suffix, StringComparison.Ordinal))
        {
            return name.Substring(0, name.Length - suffix.Length);
        }

        return name;
    }
}
