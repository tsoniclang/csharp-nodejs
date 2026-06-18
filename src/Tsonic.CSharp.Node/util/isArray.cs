namespace Tsonic.CSharp.Node;

public static partial class util
{
    /// <summary>
    /// Returns true if the given object is an Array. Otherwise, returns false.
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns>True if the object is an array.</returns>
    public static bool isArray(object? obj)
    {
        if (obj == null)
            return false;

        var type = obj.GetType();
        return type.IsArray;
    }
}
