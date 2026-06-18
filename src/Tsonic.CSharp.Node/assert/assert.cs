using System;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Provides assertion functions for testing and validation.
/// </summary>
public static partial class assert
{
    /// <summary>
    /// Tests if value is truthy.
    /// </summary>
    public static void ok(bool value, string? message = null)
    {
        if (!value)
        {
            throw new AssertionError(message, value, true, "==");
        }
    }

    /// <summary>
    /// Always fails with the provided message.
    /// </summary>
    public static void fail(string? message = null)
    {
        throw new AssertionError(message ?? "Failed");
    }

    /// <summary>
    /// Tests shallow, coercive equality between actual and expected using ==.
    /// </summary>
    public static void equal(object? actual, object? expected, string? message = null)
    {
        if (!AreLooselyEqual(actual, expected))
        {
            throw new AssertionError(message, actual, expected, "==");
        }
    }

    /// <summary>
    /// Tests shallow, coercive inequality between actual and expected using !=.
    /// </summary>
    public static void notEqual(object? actual, object? expected, string? message = null)
    {
        if (AreLooselyEqual(actual, expected))
        {
            throw new AssertionError(message, actual, expected, "!=");
        }
    }

    /// <summary>
    /// Tests strict equality between actual and expected using ===.
    /// </summary>
    public static void strictEqual(object? actual, object? expected, string? message = null)
    {
        if (!AreStrictlyEqual(actual, expected))
        {
            throw new AssertionError(message, actual, expected, "===");
        }
    }

    /// <summary>
    /// Tests strict inequality between actual and expected using !==.
    /// </summary>
    public static void notStrictEqual(object? actual, object? expected, string? message = null)
    {
        if (AreStrictlyEqual(actual, expected))
        {
            throw new AssertionError(message, actual, expected, "!==");
        }
    }

    /// <summary>
    /// Tests for deep equality between actual and expected.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JSON serialization is for comparison only")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JSON serialization is for comparison only")]
    public static void deepEqual(object? actual, object? expected, string? message = null)
    {
        if (!AreDeepEqual(actual, expected, false))
        {
            throw new AssertionError(message, actual, expected, "deepEqual");
        }
    }

    /// <summary>
    /// Tests for deep inequality between actual and expected.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JSON serialization is for comparison only")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JSON serialization is for comparison only")]
    public static void notDeepEqual(object? actual, object? expected, string? message = null)
    {
        if (AreDeepEqual(actual, expected, false))
        {
            throw new AssertionError(message, actual, expected, "notDeepEqual");
        }
    }

    /// <summary>
    /// Tests for deep strict equality between actual and expected.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JSON serialization is for comparison only")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JSON serialization is for comparison only")]
    public static void deepStrictEqual(object? actual, object? expected, string? message = null)
    {
        if (!AreDeepEqual(actual, expected, true))
        {
            throw new AssertionError(message, actual, expected, "deepEqual");
        }
    }

    /// <summary>
    /// Tests for deep strict inequality between actual and expected.
    /// </summary>
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JSON serialization is for comparison only")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JSON serialization is for comparison only")]
    public static void notDeepStrictEqual(object? actual, object? expected, string? message = null)
    {
        if (AreDeepEqual(actual, expected, true))
        {
            throw new AssertionError(message, actual, expected, "notDeepEqual");
        }
    }

    /// <summary>
    /// Expects the function fn to throw an error.
    /// </summary>
    public static void throws(Action fn, string? message = null)
    {
        try
        {
            fn();
            throw new AssertionError(message ?? "Missing expected exception", null, null, "throws");
        }
        catch (AssertionError)
        {
            throw;
        }
        catch
        {
            // Expected exception was thrown
        }
    }

    /// <summary>
    /// Expects the function fn not to throw an error.
    /// </summary>
    public static void doesNotThrow(Action fn, string? message = null)
    {
        try
        {
            fn();
        }
        catch (Exception ex)
        {
            throw new AssertionError(message ?? $"Got unwanted exception: {ex.Message}", null, null, "doesNotThrow");
        }
    }

    /// <summary>
    /// Expects the string input to match the regular expression.
    /// </summary>
    public static void match(string @string, Regex regexp, string? message = null)
    {
        if (!regexp.IsMatch(@string))
        {
            throw new AssertionError(message, @string, regexp.ToString(), "match");
        }
    }

    /// <summary>
    /// Expects the string input not to match the regular expression.
    /// </summary>
    public static void doesNotMatch(string @string, Regex regexp, string? message = null)
    {
        if (regexp.IsMatch(@string))
        {
            throw new AssertionError(message, @string, regexp.ToString(), "doesNotMatch");
        }
    }

    /// <summary>
    /// Tests if value is not null or undefined (throws if it is).
    /// </summary>
    public static void ifError(object? value)
    {
        if (value != null)
        {
            if (value is Exception ex)
            {
                throw ex;
            }
            throw new AssertionError($"ifError got unwanted exception: {value}", value, null, "ifError");
        }
    }

    /// <summary>
    /// Strict assertion alias.
    /// </summary>
    public static void strict(object? actual, object? expected, string? message = null)
    {
        strictEqual(actual, expected, message);
    }

    /// <summary>
    /// Expects the async function fn to reject.
    /// </summary>
    public static async Task rejects(Func<Task> fn, string? message = null)
    {
        if (fn == null)
            throw new ArgumentNullException(nameof(fn));

        try
        {
            await fn().ConfigureAwait(false);
            throw new AssertionError(message ?? "Missing expected rejection", null, null, "rejects");
        }
        catch (AssertionError)
        {
            throw;
        }
        catch
        {
            // Expected exception was thrown
        }
    }

    /// <summary>
    /// Expects the async function fn not to reject.
    /// </summary>
    public static async Task doesNotReject(Func<Task> fn, string? message = null)
    {
        if (fn == null)
            throw new ArgumentNullException(nameof(fn));

        try
        {
            await fn().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new AssertionError(message ?? $"Got unwanted rejection: {ex.Message}", null, null, "doesNotReject");
        }
    }

    // Helper methods for equality comparison
    private static bool AreLooselyEqual(object? a, object? b)
    {
        // In C#, we'll use Equals for loose equality
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        // Try numeric comparison if both are numbers
        if (IsNumeric(a) && IsNumeric(b))
        {
            return Convert.ToDouble(a) == Convert.ToDouble(b);
        }

        return a.Equals(b);
    }

    private static bool AreStrictlyEqual(object? a, object? b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        if (a.GetType() != b.GetType()) return false;
        return a.Equals(b);
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JSON serialization is for comparison only")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JSON serialization is for comparison only")]
    private static bool AreDeepEqual(object? a, object? b, bool strict)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;

        if (strict && a.GetType() != b.GetType()) return false;

        // For primitives and strings
        if (a is string || a.GetType().IsPrimitive)
        {
            return strict ? AreStrictlyEqual(a, b) : AreLooselyEqual(a, b);
        }

        // For complex objects, use JSON comparison
        try
        {
            var json1 = JsonSerializer.Serialize(a);
            var json2 = JsonSerializer.Serialize(b);
            return json1 == json2;
        }
        catch
        {
            return a.Equals(b);
        }
    }

    private static bool IsNumeric(object value)
    {
        return value is sbyte or byte or short or ushort or int or uint
            or long or ulong or float or double or decimal;
    }
}
