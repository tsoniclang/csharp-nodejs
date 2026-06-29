#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Collections.Generic;

namespace Tsonic.CSharp.Node;

public sealed class ParseOptions
{
    public Func<string, string>? decodeURIComponent { get; set; }

    public int maxKeys { get; set; } = 1000;
}

public sealed class StringifyOptions
{
    public Func<string, string>? encodeURIComponent { get; set; }
}

public static partial class querystring
{
    public static byte[] unescapeBuffer(string input)
    {
        return System.Text.Encoding.UTF8.GetBytes(unescape(input));
    }

    public static Dictionary<string, object> parse(string str, string? sep, string? eq, ParseOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        return parseWithDecoder(str, sep, eq, options.maxKeys, options.decodeURIComponent ?? unescape);
    }

    public static string stringify(Dictionary<string, object?>? obj, string? sep, string? eq, StringifyOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        return stringifyWithEncoder(obj, sep, eq, options.encodeURIComponent ?? escape);
    }
}
