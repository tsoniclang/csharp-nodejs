using System;
using System.Net.Http;

namespace Tsonic.CSharp.Node.Http;

/// <summary>
/// Implements Node.js http module functionality using Kestrel and HttpClient.
/// All methods follow JavaScript naming conventions (lowercase).
/// </summary>
public static partial class http
{
    private static readonly HttpClient _sharedHttpClient = new();

    /// <summary>
    /// Global maximum number of sockets to allow per host.
    /// Default: Infinity (no limit in .NET)
    /// </summary>
    public static int? globalAgent_maxSockets { get; set; } = null;

    /// <summary>
    /// Global maximum number of free sockets per host to leave open.
    /// Default: 256
    /// </summary>
    public static int globalAgent_maxFreeSockets { get; set; } = 256;

    /// <summary>
    /// Global keep-alive timeout in milliseconds.
    /// Default: 5000
    /// </summary>
    public static int globalAgent_timeout { get; set; } = 5000;

    /// <summary>
    /// Maximum allowed size of HTTP headers in bytes.
    /// Default: 16KB (matches Node.js)
    /// </summary>
    public static int maxHeaderSize { get; set; } = 16 * 1024;

    /// <summary>
    /// Creates a new HTTP server.
    /// </summary>
    /// <param name="requestListener">Optional request handler function.</param>
    /// <returns>A new Server instance.</returns>
    public static Server createServer(Action<IncomingMessage, ServerResponse>? requestListener = null)
    {
        return new Server(requestListener);
    }

    /// <summary>
    /// Makes an HTTP request.
    /// </summary>
    /// <param name="options">Request options.</param>
    /// <param name="callback">Optional callback for response.</param>
    /// <returns>A ClientRequest instance.</returns>
    public static ClientRequest request(RequestOptions options, Action<IncomingMessage>? callback = null)
    {
        return new ClientRequest(_sharedHttpClient, options, callback);
    }

    /// <summary>
    /// Makes an HTTP request from a URL string.
    /// </summary>
    /// <param name="url">The URL to request.</param>
    /// <param name="callback">Optional callback for response.</param>
    /// <returns>A ClientRequest instance.</returns>
    public static ClientRequest request(string url, Action<IncomingMessage>? callback = null)
    {
        var uri = new Uri(url);
        var options = new RequestOptions
        {
            hostname = uri.Host,
            port = uri.Port,
            path = uri.PathAndQuery,
            method = "GET"
        };
        return new ClientRequest(_sharedHttpClient, options, callback);
    }

    /// <summary>
    /// Makes a GET request.
    /// Convenience method that calls request() and automatically calls req.end().
    /// </summary>
    /// <param name="url">The URL to request.</param>
    /// <param name="callback">Optional callback for response.</param>
    /// <returns>A ClientRequest instance.</returns>
    public static ClientRequest get(string url, Action<IncomingMessage>? callback = null)
    {
        var req = request(url, callback);
        _ = req.end();
        return req;
    }

    /// <summary>
    /// Makes a GET request with options.
    /// Convenience method that calls request() and automatically calls req.end().
    /// </summary>
    /// <param name="options">Request options.</param>
    /// <param name="callback">Optional callback for response.</param>
    /// <returns>A ClientRequest instance.</returns>
    public static ClientRequest get(RequestOptions options, Action<IncomingMessage>? callback = null)
    {
        var req = request(options, callback);
        _ = req.end();
        return req;
    }

    /// <summary>
    /// Validates that the given name is a valid HTTP header name.
    /// Throws TypeError if invalid.
    /// </summary>
    /// <param name="name">Header name to validate.</param>
    public static void validateHeaderName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new TypeError("Header name must be a valid string");

        // Check for invalid characters
        // Valid: alphanumeric, !, #, $, %, &, ', *, +, -, ., ^, _, `, |, ~
        foreach (char c in name)
        {
            if (!IsValidHeaderNameChar(c))
                throw new TypeError($"Invalid character in header name: '{c}'");
        }
    }

    /// <summary>
    /// Validates that the given value is a valid HTTP header value.
    /// Throws TypeError if invalid.
    /// </summary>
    /// <param name="name">Header name (for error messages).</param>
    /// <param name="value">Header value to validate.</param>
    public static void validateHeaderValue(string name, object? value)
    {
        if (value == null)
            throw new TypeError($"Invalid value for header '{name}': value cannot be null");

        var strValue = value.ToString();
        if (strValue == null)
            throw new TypeError($"Invalid value for header '{name}': value cannot be null");

        // Check for invalid characters (control characters except tab)
        foreach (char c in strValue)
        {
            if (c < 0x20 && c != '\t' || c == 0x7F)
                throw new TypeError($"Invalid character in header '{name}' value");
        }
    }

    private static bool IsValidHeaderNameChar(char c)
    {
        return (c >= 'a' && c <= 'z') ||
               (c >= 'A' && c <= 'Z') ||
               (c >= '0' && c <= '9') ||
               c == '!' || c == '#' || c == '$' || c == '%' || c == '&' || c == '\'' ||
               c == '*' || c == '+' || c == '-' || c == '.' || c == '^' || c == '_' ||
               c == '`' || c == '|' || c == '~';
    }
}

/// <summary>
/// TypeError exception for HTTP errors.
/// </summary>
public class TypeError : Exception
{
    /// <summary>
    /// Creates a new TypeError with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public TypeError(string message) : base(message) { }
}
