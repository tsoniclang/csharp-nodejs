using System;
using System.Net.Http;
using Tsonic.CSharp.Node;
using Tsonic.CSharp.Node.Http;

namespace Tsonic.CSharp.Node.Https;

#pragma warning disable CS1591

/// <summary>
/// HTTPS client and server helpers.
/// </summary>
public static class https
{
    private static readonly HttpClient SharedHttpClient = new();

    public static Http.Server createServer(Action<IncomingMessage, ServerResponse>? requestListener = null)
    {
        return new Http.Server(requestListener);
    }

    public static Http.Server createServer(HttpsServerOptions options, Action<IncomingMessage, ServerResponse>? requestListener = null)
    {
        _ = options;
        return new Http.Server(requestListener);
    }

    public static ClientRequest request(string url, Action<IncomingMessage>? callback = null)
    {
        var uri = new Uri(url);
        if (!string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("HTTPS request URL must use the https: protocol.", nameof(url));

        return request(new RequestOptions
        {
            hostname = uri.Host,
            port = uri.IsDefaultPort ? 443 : uri.Port,
            path = uri.PathAndQuery,
            method = "GET",
            protocol = "https:"
        }, callback);
    }

    public static ClientRequest request(RequestOptions options, Action<IncomingMessage>? callback = null)
    {
        var normalized = NormalizeHttpsOptions(options);
        return new ClientRequest(SharedHttpClient, normalized, callback);
    }

    public static ClientRequest get(string url, Action<IncomingMessage>? callback = null)
    {
        var req = request(url, callback);
        _ = req.end();
        return req;
    }

    public static ClientRequest get(RequestOptions options, Action<IncomingMessage>? callback = null)
    {
        var req = request(options, callback);
        _ = req.end();
        return req;
    }

    private static RequestOptions NormalizeHttpsOptions(RequestOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        options.protocol = "https:";
        if (options.port == 80)
            options.port = 443;

        return options;
    }
}

public sealed class HttpsServerOptions : TlsOptions
{
    public int? maxHeaderSize { get; set; }
}
