namespace Tsonic.CSharp.Node;

public static partial class util
{
    /// <summary>
    /// Returns a string representation of an object that is intended for debugging.
    /// </summary>
    /// <param name="obj">The object to inspect.</param>
    /// <returns>A string representation of the object.</returns>
    public static string inspect(object? obj)
    {
        _ = obj;
        throw UnsupportedOpenCarrierOperation("node:util.inspect");
    }
}
