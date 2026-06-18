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
    /// Parses URL input and returns URL instance.
    /// </summary>
    public static URL? parse(string input)
    {
        return URL.parse(input);
    }

    /// <summary>
    /// Formats URL input to string.
    /// </summary>
    public static string format(object? input)
    {
        if (input == null)
            return string.Empty;

        if (input is URL parsedUrl)
            return parsedUrl.href;

        if (input is string inputString)
            return URL.parse(inputString)?.href ?? inputString;

        return input.ToString() ?? string.Empty;
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
    public static string fileURLToPath(URL fileUrl)
    {
        if (fileUrl == null)
            throw new ArgumentNullException(nameof(fileUrl));

        return fileURLToPath(fileUrl.href);
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
}
