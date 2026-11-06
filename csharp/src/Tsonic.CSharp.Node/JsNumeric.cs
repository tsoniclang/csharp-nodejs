using System;

namespace Tsonic.CSharp.Node;

/// <summary>Native integer validation for Node capability operations.</summary>
public static class JsNumeric
{
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
