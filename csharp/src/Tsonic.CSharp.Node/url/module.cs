using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Legacy url module helpers.
/// </summary>
public static class url
{
    /// <summary>
    /// Converts a domain to ASCII using IDN rules.
    /// </summary>
    public static string domainToASCII(string domain)
    {
        if (string.IsNullOrEmpty(domain))
            return string.Empty;

        try
        {
            return new IdnMapping().GetAscii(domain);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Converts a punycode/ASCII domain to Unicode.
    /// </summary>
    public static string domainToUnicode(string domain)
    {
        if (string.IsNullOrEmpty(domain))
            return string.Empty;

        try
        {
            return new IdnMapping().GetUnicode(domain);
        }
        catch
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Parses URL input and returns the legacy Node.js URL object shape.
    /// </summary>
    public static LegacyUrlObject parse(string input)
    {
        return parse(input, false, false);
    }

    /// <summary>
    /// Parses URL input and returns a legacy URL object shape.
    /// </summary>
    public static LegacyUrlObject parse(string input, bool parseQueryString, bool slashesDenoteHost = false)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));
        if (input.IndexOf('\0') >= 0)
            throw new UriFormatException("URL input contains a null character.");

        var hashIndex = input.IndexOf('#');
        var hash = hashIndex < 0 ? null : input[hashIndex..];
        var beforeHash = hashIndex < 0 ? input : input[..hashIndex];
        var queryIndex = beforeHash.IndexOf('?');
        var search = queryIndex < 0 ? null : beforeHash[queryIndex..];
        var beforeQuery = queryIndex < 0 ? beforeHash : beforeHash[..queryIndex];

        string? protocol = null;
        var remainder = beforeQuery;
        var colonIndex = beforeQuery.IndexOf(':');
        if (colonIndex > 0 && IsUrlScheme(beforeQuery.AsSpan(0, colonIndex)))
        {
            protocol = beforeQuery[..(colonIndex + 1)].ToLowerInvariant();
            remainder = beforeQuery[(colonIndex + 1)..];
        }

        var hasAuthority = remainder.StartsWith("//", StringComparison.Ordinal) &&
            (protocol is not null || slashesDenoteHost);
        string? auth = null;
        string? host = null;
        string? hostname = null;
        string? port = null;
        string? pathname = remainder.Length == 0 ? null : remainder;
        if (hasAuthority)
        {
            remainder = remainder[2..];
            var slashIndex = remainder.IndexOf('/');
            var authority = slashIndex < 0 ? remainder : remainder[..slashIndex];
            pathname = slashIndex < 0 ? "/" : remainder[slashIndex..];
            var atIndex = authority.LastIndexOf('@');
            if (atIndex >= 0)
            {
                auth = authority[..atIndex];
                authority = authority[(atIndex + 1)..];
            }
            host = authority;
            SplitHost(authority, out hostname, out port);
        }

        var queryText = search?.StartsWith("?", StringComparison.Ordinal) == true ? search[1..] : search;
        var parsed = new LegacyUrlObject
        {
            protocol = protocol,
            auth = auth,
            host = host,
            hostname = hostname,
            port = port,
            pathname = pathname,
            search = search,
            query = parseQueryString ? ToQueryObject(new URLSearchParams(queryText ?? string.Empty)) : queryText,
            hash = hash,
            path = pathname is null && search is null ? null : (pathname ?? string.Empty) + (search ?? string.Empty),
            slashes = hasAuthority ? true : null
        };
        parsed.href = format(parsed);
        return parsed;
    }

    private static bool IsUrlScheme(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty || !char.IsAsciiLetter(value[0]))
            return false;
        for (var index = 1; index < value.Length; index++)
        {
            var character = value[index];
            if (!char.IsAsciiLetterOrDigit(character) && character != '+' && character != '-' && character != '.')
                return false;
        }
        return true;
    }

    private static void SplitHost(string authority, out string hostname, out string port)
    {
        hostname = authority;
        port = string.Empty;
        if (authority.StartsWith("[", StringComparison.Ordinal))
        {
            var closeBracket = authority.IndexOf(']');
            if (closeBracket >= 0)
            {
                hostname = authority[..(closeBracket + 1)];
                if (closeBracket + 1 < authority.Length && authority[closeBracket + 1] == ':')
                    port = authority[(closeBracket + 2)..];
            }
            return;
        }
        var colon = authority.LastIndexOf(':');
        if (colon > 0 && authority.IndexOf(':') == colon)
        {
            hostname = authority[..colon];
            port = authority[(colon + 1)..];
        }
    }

    /// <summary>
    /// Formats URL input to string.
    /// </summary>
    public static string format(LegacyUrlObject input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var protocol = input.protocol ?? string.Empty;
        if (protocol.Length > 0 && !protocol.EndsWith(':'))
            protocol += ":";
        var host = !string.IsNullOrEmpty(input.host)
            ? input.host
            : (input.hostname ?? string.Empty) +
                (string.IsNullOrEmpty(input.port) ? string.Empty : ":" + input.port);
        var slashes = input.slashes == true ||
            (IsSlashedProtocol(protocol) && (host.Length > 0 || protocol == "file:"));
        var result = protocol;
        if (slashes)
            result += "//";
        if (!string.IsNullOrEmpty(input.auth))
            result += input.auth + "@";
        result += host;
        var pathname = input.pathname ?? string.Empty;
        if (slashes && host.Length > 0 && pathname.Length > 0 && !pathname.StartsWith('/'))
            pathname = "/" + pathname;
        result += pathname;
        result += PrefixIfPresent(input.search, '?');
        result += PrefixIfPresent(input.hash, '#');
        return result;
    }

    private static bool IsSlashedProtocol(string protocol) => protocol is
        "http:" or "https:" or "ftp:" or "gopher:" or "file:" or "ws:" or "wss:";

    private static string PrefixIfPresent(string? value, char prefix)
    {
        if (string.IsNullOrEmpty(value) || value[0] == prefix)
            return value ?? string.Empty;
        return prefix + value;
    }

    /// <summary>
    /// Formats URL input with legacy formatting options.
    /// </summary>
    public static string format(URL input, URLFormatOptions? options = null)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        options ??= new URLFormatOptions();
        var result = input.href;
        if (!options.fragment && !string.IsNullOrEmpty(input.hash))
            result = result.Replace(input.hash, string.Empty, StringComparison.Ordinal);
        if (!options.search && !string.IsNullOrEmpty(input.search))
            result = result.Replace(input.search, string.Empty, StringComparison.Ordinal);
        return result;
    }

    /// <summary>
    /// Resolves relative URL against a base URL.
    /// </summary>
    public static string resolve(string from, string to)
    {
        var baseUri = new Uri(from, UriKind.Absolute);
        var resolved = new Uri(baseUri, to);
        return resolved.ToString();
    }

    /// <summary>
    /// Converts a file path to a file URL.
    /// </summary>
    public static URL pathToFileURL(string filePath)
    {
        var fullPath = Path.GetFullPath(filePath);
        return new URL(new Uri(fullPath).AbsoluteUri);
    }

    /// <summary>
    /// Converts a file path to a file URL.
    /// </summary>
    public static URL pathToFileURL(string filePath, PathToFileUrlOptions? options)
    {
        _ = options;
        return pathToFileURL(filePath);
    }

    /// <summary>
    /// Converts a file URL to filesystem path.
    /// </summary>
    public static string fileURLToPath(string fileUrl)
    {
        var uri = new Uri(fileUrl, UriKind.Absolute);
        return uri.LocalPath;
    }

    /// <summary>
    /// Converts a file URL to filesystem path.
    /// </summary>
    public static string fileURLToPath(string fileUrl, FileUrlToPathOptions? options)
    {
        _ = options;
        return fileURLToPath(fileUrl);
    }

    /// <summary>
    /// Converts a file URL to filesystem path.
    /// </summary>
    public static string fileURLToPath(URL fileUrl)
    {
        if (fileUrl == null)
            throw new ArgumentNullException(nameof(fileUrl));

        return fileURLToPath(fileUrl.href);
    }

    /// <summary>
    /// Converts a file URL to filesystem path.
    /// </summary>
    public static string fileURLToPath(URL fileUrl, FileUrlToPathOptions? options)
    {
        _ = options;
        return fileURLToPath(fileUrl);
    }

    /// <summary>
    /// Converts a file URL to UTF-8 encoded path buffer.
    /// </summary>
    public static Buffer fileURLToPathBuffer(string fileUrl)
    {
        var pathText = fileURLToPath(fileUrl);
        return Buffer.from(Encoding.UTF8.GetBytes(pathText));
    }

    /// <summary>
    /// Converts a file URL to UTF-8 encoded path buffer.
    /// </summary>
    public static Buffer fileURLToPathBuffer(URL fileUrl)
    {
        if (fileUrl == null)
            throw new ArgumentNullException(nameof(fileUrl));

        return fileURLToPathBuffer(fileUrl.href);
    }

    /// <summary>
    /// Converts URL to HTTP request option dictionary.
    /// </summary>
    public static Dictionary<string, object?> urlToHttpOptions(URL input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        var options = new Dictionary<string, object?>
        {
            ["protocol"] = input.protocol,
            ["hostname"] = input.hostname,
            ["hash"] = input.hash,
            ["search"] = input.search,
            ["pathname"] = input.pathname,
            ["path"] = input.pathname + input.search,
            ["href"] = input.href,
        };

        if (!string.IsNullOrEmpty(input.port) && int.TryParse(input.port, out var port))
        {
            options["port"] = port;
        }

        return options;
    }

    /// <summary>
    /// Converts URL to HTTP request option carrier.
    /// </summary>
    public static HttpOptions urlToHttpOptionsObject(URL input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        return new HttpOptions
        {
            protocol = input.protocol,
            hostname = input.hostname,
            host = input.host,
            port = string.IsNullOrEmpty(input.port) ? null : int.Parse(input.port, CultureInfo.InvariantCulture),
            path = input.pathname + input.search,
            href = input.href,
            auth = string.IsNullOrEmpty(input.username) && string.IsNullOrEmpty(input.password) ? null : $"{input.username}:{input.password}"
        };
    }

    private static Dictionary<string, string[]> ToQueryObject(URLSearchParams searchParams)
    {
        var result = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        foreach (var entry in searchParams.entries())
        {
            if (!result.TryGetValue(entry.Key, out var values))
            {
                values = [];
                result[entry.Key] = values;
            }

            values.Add(entry.Value);
        }

        return result.ToDictionary(item => item.Key, item => item.Value.ToArray(), StringComparer.Ordinal);
    }
}
