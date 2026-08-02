using System;

namespace Tsonic.CSharp.Node;

internal static class JsNumeric
{
    public static int RequirePort(double value, string paramName)
    {
        if (!double.IsFinite(value) || Math.Truncate(value) != value || value < 0 || value > 65535)
        {
            throw new ArgumentOutOfRangeException(paramName, "Port must be an integer between 0 and 65535.");
        }

        return checked((int)value);
    }

    public static int RequireNonNegativeInt(int value, string paramName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "Value must be non-negative.");
        }

        return value;
    }
}
