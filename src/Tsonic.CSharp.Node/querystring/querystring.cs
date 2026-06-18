using System;
using System.Collections.Generic;
using System.Text;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Utilities for parsing and formatting URL query strings.
/// </summary>
public static class querystring
{
    /// <summary>
    /// Produces a URL query string from a given object by iterating through the object's properties.
    /// </summary>
    /// <param name="obj">The object to serialize into a URL query string.</param>
    /// <param name="sep">The substring used to delimit key and value pairs in the query string. Default is '&amp;'.</param>
    /// <param name="eq">The substring used to delimit keys and values in the query string. Default is '='.</param>
    /// <returns>A URL query string.</returns>
    public static string stringify(Dictionary<string, object?>? obj, string? sep = null, string? eq = null)
    {
        if (obj == null || obj.Count == 0)
            return string.Empty;

        sep ??= "&";
        eq ??= "=";

        var sb = new StringBuilder();
        bool first = true;

        foreach (var kvp in obj)
        {
            var key = escape(kvp.Key);

            if (kvp.Value is Array arr)
            {
                foreach (var item in arr)
                {
                    if (!first)
                        sb.Append(sep);
                    first = false;

                    sb.Append(key);
                    sb.Append(eq);
                    sb.Append(escape(ConvertToString(item)));
                }
            }
            else if (kvp.Value is System.Collections.IEnumerable enumerable and not string)
            {
                foreach (var item in enumerable)
                {
                    if (!first)
                        sb.Append(sep);
                    first = false;

                    sb.Append(key);
                    sb.Append(eq);
                    sb.Append(escape(ConvertToString(item)));
                }
            }
            else
            {
                if (!first)
                    sb.Append(sep);
                first = false;

                sb.Append(key);
                sb.Append(eq);
                sb.Append(escape(ConvertToString(kvp.Value)));
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Parses a URL query string into a collection of key and value pairs.
    /// </summary>
    /// <param name="str">The URL query string to parse.</param>
    /// <param name="sep">The substring used to delimit key and value pairs in the query string. Default is '&amp;'.</param>
    /// <param name="eq">The substring used to delimit keys and values in the query string. Default is '='.</param>
    /// <param name="maxKeys">Specifies the maximum number of keys to parse. Specify 0 to remove key counting limitations. Default is 1000.</param>
    /// <returns>A dictionary of key-value pairs.</returns>
    public static Dictionary<string, object> parse(string str, string? sep = null, string? eq = null, int maxKeys = 1000)
    {
        var result = new Dictionary<string, object>();

        if (string.IsNullOrEmpty(str))
            return result;

        // Remove leading '?' if present
        if (str.StartsWith('?'))
            str = str.Substring(1);

        sep ??= "&";
        eq ??= "=";

        var pairs = str.Split(new[] { sep }, StringSplitOptions.None);
        int count = 0;

        foreach (var pair in pairs)
        {
            if (maxKeys > 0 && count >= maxKeys)
                break;

            var eqIndex = pair.IndexOf(eq);
            string key, value;

            if (eqIndex >= 0)
            {
                key = unescape(pair.Substring(0, eqIndex));
                value = unescape(pair.Substring(eqIndex + eq.Length));
            }
            else
            {
                key = unescape(pair);
                value = "";
            }

            if (result.ContainsKey(key))
            {
                // Convert to array or append to existing array
                if (result[key] is List<string> list)
                {
                    list.Add(value);
                }
                else if (result[key] is string existingValue)
                {
                    result[key] = new List<string> { existingValue, value };
                }
            }
            else
            {
                result[key] = value;
            }

            count++;
        }

        // Convert Lists to string arrays
        var keysToConvert = new List<string>();
        foreach (var kvp in result)
        {
            if (kvp.Value is List<string>)
                keysToConvert.Add(kvp.Key);
        }

        foreach (var key in keysToConvert)
        {
            result[key] = ((List<string>)result[key]).ToArray();
        }

        return result;
    }

    /// <summary>
    /// Alias for stringify().
    /// </summary>
    public static string encode(Dictionary<string, object?>? obj, string? sep = null, string? eq = null)
    {
        return stringify(obj, sep, eq);
    }

    /// <summary>
    /// Alias for parse().
    /// </summary>
    public static Dictionary<string, object> decode(string str, string? sep = null, string? eq = null, int maxKeys = 1000)
    {
        return parse(str, sep, eq, maxKeys);
    }

    /// <summary>
    /// Performs URL percent-encoding on the given string.
    /// </summary>
    /// <param name="str">The string to encode.</param>
    /// <returns>The percent-encoded string.</returns>
    public static string escape(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        return Uri.EscapeDataString(str);
    }

    /// <summary>
    /// Performs decoding of URL percent-encoded characters on the given string.
    /// </summary>
    /// <param name="str">The string to decode.</param>
    /// <returns>The decoded string.</returns>
    public static string unescape(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        try
        {
            return Uri.UnescapeDataString(str);
        }
        catch
        {
            // If decoding fails, return the original string (safer equivalent)
            return str;
        }
    }

    private static string ConvertToString(object? value)
    {
        if (value == null)
            return "";

        return value.ToString() ?? "";
    }
}
