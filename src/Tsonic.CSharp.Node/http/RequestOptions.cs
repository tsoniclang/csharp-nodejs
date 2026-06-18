using System;
using System.Collections.Generic;

namespace Tsonic.CSharp.Node.Http;

/// <summary>
/// Options for making HTTP requests.
/// Matches Node.js http.RequestOptions interface.
/// </summary>
public class RequestOptions
{
    /// <summary>
    /// Domain name or IP address of the server. Default: 'localhost'
    /// </summary>
    public string? hostname { get; set; }

    /// <summary>
    /// Alias for hostname.
    /// </summary>
    public string? host
    {
        get => hostname;
        set => hostname = value;
    }

    /// <summary>
    /// Port of remote server. Default: 80
    /// </summary>
    public int port { get; set; } = 80;

    /// <summary>
    /// Request path. Should include query string if any. Default: '/'
    /// </summary>
    public string? path { get; set; } = "/";

    /// <summary>
    /// HTTP request method. Default: 'GET'
    /// </summary>
    public string method { get; set; } = "GET";

    /// <summary>
    /// Object containing request headers.
    /// </summary>
    public Dictionary<string, string>? headers { get; set; }

    /// <summary>
    /// Protocol to use. Default: 'http:'
    /// </summary>
    public string protocol { get; set; } = "http:";

    /// <summary>
    /// Request timeout in milliseconds. Default: no timeout
    /// </summary>
    public int? timeout { get; set; }

    /// <summary>
    /// Controls Agent behavior. Possible values:
    /// - null (default): use global agent
    /// - Agent instance: explicitly use passed Agent
    /// - false: disable connection pooling
    /// </summary>
    public object? agent { get; set; }

    /// <summary>
    /// Authentication in the form 'user:password' for basic auth.
    /// </summary>
    public string? auth { get; set; }
}
