using System.Text;
using System.Diagnostics.CodeAnalysis;

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
    [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "JSON serialization is for debugging only, fallback available")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "JSON serialization is for debugging only, fallback available")]
    public static string format(object? format, params object?[] args)
    {
        if (format == null)
            return string.Empty;

        var formatStr = format.ToString() ?? string.Empty;

        var result = new StringBuilder();
        int argIndex = 0;
        int i = 0;

        while (i < formatStr.Length)
        {
            if (formatStr[i] == '%' && i + 1 < formatStr.Length)
            {
                char specifier = formatStr[i + 1];

                // Handle %% first (doesn't require an argument)
                if (specifier == '%')
                {
                    result.Append('%');
                    i += 2;
                    continue;
                }

                // Other placeholders require arguments
                if (argIndex < args.Length)
                {
                    switch (specifier)
                    {
                        case 's': // string
                            result.Append(args[argIndex]?.ToString() ?? "");
                            argIndex++;
                            i += 2;
                            break;
                        case 'd': // number
                            result.Append(args[argIndex]?.ToString() ?? "");
                            argIndex++;
                            i += 2;
                            break;
                        case 'i': // integer
                            result.Append(args[argIndex]?.ToString() ?? "");
                            argIndex++;
                            i += 2;
                            break;
                        case 'f': // float
                            result.Append(args[argIndex]?.ToString() ?? "");
                            argIndex++;
                            i += 2;
                            break;
                        case 'j': // JSON
                            try
                            {
                                result.Append(System.Text.Json.JsonSerializer.Serialize(args[argIndex]));
                            }
                            catch
                            {
                                result.Append(args[argIndex]?.ToString() ?? "");
                            }
                            argIndex++;
                            i += 2;
                            break;
                        case 'o': // object
                        case 'O': // object with options
                            result.Append(inspect(args[argIndex]));
                            argIndex++;
                            i += 2;
                            break;
                        default:
                            result.Append(formatStr[i]);
                            i++;
                            break;
                    }
                }
                else
                {
                    result.Append(formatStr[i]);
                    i++;
                }
            }
            else
            {
                result.Append(formatStr[i]);
                i++;
            }
        }

        // Append remaining arguments
        while (argIndex < args.Length)
        {
            result.Append(' ');
            result.Append(args[argIndex]?.ToString() ?? "");
            argIndex++;
        }

        return result.ToString();
    }
}
