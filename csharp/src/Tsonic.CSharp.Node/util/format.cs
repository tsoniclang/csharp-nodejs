namespace Tsonic.CSharp.Node;

public static partial class util
{
    /// <summary>
    /// Returns a formatted string using the first argument as a printf-like format string.
    /// Supports %s (string), %d (number), %j (JSON), %% (literal percent).
    /// </summary>
    /// <param name="format">The format string.</param>
    /// <param name="args">Values to format.</param>
    /// <returns>The formatted string.</returns>
    public static string format(object? format, params object?[] args)
    {
        _ = format;
        _ = args;
        throw UnsupportedOpenCarrierOperation("node:util.format");
    }
}
