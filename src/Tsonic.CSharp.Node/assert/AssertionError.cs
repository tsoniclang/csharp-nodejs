using System;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Indicates the failure of an assertion.
/// </summary>
public class AssertionError : Exception
{
    /// <summary>
    /// The actual value in the assertion.
    /// </summary>
    public object? actual { get; set; }

    /// <summary>
    /// The expected value in the assertion.
    /// </summary>
    public object? expected { get; set; }

    /// <summary>
    /// The operator used in the assertion.
    /// </summary>
    public string @operator { get; set; }

    /// <summary>
    /// Indicates if the message was auto-generated.
    /// </summary>
    public bool generatedMessage { get; set; }

    /// <summary>
    /// The error code (always "ERR_ASSERTION").
    /// </summary>
    public string code => "ERR_ASSERTION";

    /// <summary>
    /// Creates a new AssertionError.
    /// </summary>
    public AssertionError(string? message, object? actual = null, object? expected = null, string @operator = "")
        : base(message ?? GenerateMessage(actual, expected, @operator))
    {
        this.actual = actual;
        this.expected = expected;
        this.@operator = @operator;
        this.generatedMessage = message == null;
    }

    private static string GenerateMessage(object? actual, object? expected, string @operator)
    {
        return @operator switch
        {
            "==" => $"Expected {FormatValue(actual)} == {FormatValue(expected)}",
            "!=" => $"Expected {FormatValue(actual)} != {FormatValue(expected)}",
            "===" => $"Expected {FormatValue(actual)} === {FormatValue(expected)}",
            "!==" => $"Expected {FormatValue(actual)} !== {FormatValue(expected)}",
            "deepEqual" => $"Expected values to be deeply equal:\n{FormatValue(actual)}\nvs\n{FormatValue(expected)}",
            "notDeepEqual" => $"Expected values not to be deeply equal",
            "throws" => "Missing expected exception",
            "doesNotThrow" => "Got unwanted exception",
            _ => $"Assertion failed: {FormatValue(actual)}"
        };
    }

    private static string FormatValue(object? value)
    {
        if (value == null) return "null";
        if (value is string str) return $"\"{str}\"";
        return value.ToString() ?? "undefined";
    }
}
