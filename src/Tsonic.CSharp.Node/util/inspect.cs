using System.Text.Json;
using System.Diagnostics.CodeAnalysis;

namespace Tsonic.CSharp.Node;

public static partial class util
{
    /// <summary>
    /// Returns a string representation of an object that is intended for debugging.
    /// </summary>
    /// <param name="obj">The object to inspect.</param>
    /// <returns>A string representation of the object.</returns>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JSON serialization is for debugging only, fallback available")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JSON serialization is for debugging only, fallback available")]
    public static string inspect(object? obj)
    {
        if (obj == null)
            return "null";

        if (obj is string str)
            return $"'{str}'";

        if (obj is bool b)
            return b ? "true" : "false";

        if (obj is int || obj is long || obj is short || obj is byte ||
            obj is uint || obj is ulong || obj is ushort || obj is sbyte ||
            obj is float || obj is double || obj is decimal)
            return obj.ToString() ?? "";

        // Try JSON serialization for complex objects
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(obj, options);
        }
        catch
        {
            return obj.ToString() ?? "";
        }
    }
}
