using System.Text.Json;
using System.Diagnostics.CodeAnalysis;

namespace Tsonic.CSharp.Node;

public static partial class util
{
    /// <summary>
    /// Returns true if there is deep strict equality between val1 and val2. Otherwise, returns false.
    /// </summary>
    /// <param name="val1">The first value to compare.</param>
    /// <param name="val2">The second value to compare.</param>
    /// <returns>True if the values are deeply equal.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JSON serialization is for deep equality check only, fallback available")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JSON serialization is for deep equality check only, fallback available")]
    public static bool isDeepStrictEqual(object? val1, object? val2)
    {
        // Null checks
        if (val1 == null && val2 == null)
            return true;
        if (val1 == null || val2 == null)
            return false;

        // Same reference
        if (ReferenceEquals(val1, val2))
            return true;

        // Different types
        if (val1.GetType() != val2.GetType())
            return false;

        // Primitive types and strings
        if (val1 is string || val1.GetType().IsPrimitive)
            return val1.Equals(val2);

        // For complex objects, use JSON comparison (simple approach)
        try
        {
            var json1 = JsonSerializer.Serialize(val1);
            var json2 = JsonSerializer.Serialize(val2);
            return json1 == json2;
        }
        catch
        {
            return val1.Equals(val2);
        }
    }
}
