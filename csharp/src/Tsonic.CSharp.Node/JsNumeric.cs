using System;

namespace Tsonic.CSharp.Node;

/// <summary>Checked numeric adapters for generated Node capability calls.</summary>
public static class JsNumeric
{
    /// <summary>Accepts an exact finite integer within the native Int32 range.</summary>
    public static int RequireInteger(double value)
    {
        if (!double.IsInteger(value))
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be an integer.");
        return checked((int)value);
    }

    internal static int RequirePort(int value, string paramName)
    {
        if (value < 0 || value > 65535)
        {
            throw new ArgumentOutOfRangeException(paramName, "Port must be an integer between 0 and 65535.");
        }

        return value;
    }

    internal static int RequireNonNegativeInt(int value, string paramName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Value must be non-negative.");
        }

        return value;
    }
}
