using System.Text;
using System.Text.RegularExpressions;

namespace Tsonic.CSharp.Node;

public static partial class util
{
    /// <summary>
    /// Formats a string with options (options are currently ignored).
    /// </summary>
    public static string formatWithOptions(object? inspectOptions, object? formatValue, params object?[] args)
    {
        _ = inspectOptions;
        return format(formatValue, args);
    }

    /// <summary>
    /// Removes VT control sequences from a string.
    /// </summary>
    public static string stripVTControlCharacters(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        return Regex.Replace(input, @"\x1B\[[0-?]*[ -/]*[@-~]", string.Empty);
    }

    /// <summary>
    /// Converts string to a USVString by replacing lone surrogates.
    /// </summary>
    public static string toUSVString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var sb = new StringBuilder(input.Length);
        for (var i = 0; i < input.Length; i++)
        {
            var ch = input[i];
            if (!char.IsSurrogate(ch))
            {
                sb.Append(ch);
                continue;
            }

            if (char.IsHighSurrogate(ch) && i + 1 < input.Length && char.IsLowSurrogate(input[i + 1]))
            {
                sb.Append(ch);
                sb.Append(input[++i]);
                continue;
            }

            sb.Append('\uFFFD');
        }

        return sb.ToString();
    }
}
