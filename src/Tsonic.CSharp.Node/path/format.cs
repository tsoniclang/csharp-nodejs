namespace Tsonic.CSharp.Node;

public static partial class path
{
    /// <summary>
    /// Returns a path string from an object - the opposite of parse().
    /// </summary>
    /// <param name="pathObject">The path object to format.</param>
    /// <returns>The formatted path string.</returns>
    public static string format(ParsedPath pathObject)
    {
        if (pathObject == null)
            return string.Empty;

        var dir = pathObject.dir ?? string.Empty;
        var @base = pathObject.@base ?? string.Empty;

        if (string.IsNullOrEmpty(dir))
        {
            return @base;
        }

        if (dir == pathObject.root)
        {
            return dir + @base;
        }

        return Path.Combine(dir, @base);
    }
}
