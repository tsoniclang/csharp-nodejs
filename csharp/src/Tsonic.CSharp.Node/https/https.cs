using System;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Tsonic.CSharp.Node;
using Tsonic.CSharp.Node.Http;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node.Https;

#pragma warning disable CS1591

/// <summary>
/// HTTPS client and server helpers.
/// </summary>
public static class https
{
    public static Http.Server createServer(Action<IncomingMessage, ServerResponse>? requestListener = null)
    {
        throw new ArgumentException("HTTPS server options must supply an exact certificate and private key.");
    }

    public static Http.Server createServer(HttpsServerOptions options, Action<IncomingMessage, ServerResponse>? requestListener = null)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));
        var context = tls.createSecureContext(new SecureContextOptions
        {
            cert = options.cert,
            key = options.key,
            ca = options.ca,
            pfx = options.pfx,
            passphrase = options.passphrase,
            minVersion = options.minVersion,
            maxVersion = options.maxVersion
        });
        var certificate = context.Certificate
            ?? throw new ArgumentException("HTTPS server options require a valid certificate and private key.", nameof(options));
        return new Http.Server(
            requestListener,
            listenOptions => listenOptions.UseHttps(certificate));
    }

    public static ClientRequest request(string url, Action<IncomingMessage>? callback = null)
    {
        var uri = new Uri(url);
        if (!string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("HTTPS request URL must use the https: protocol.", nameof(url));

        return request(new HttpsRequestOptions
        {
            hostname = uri.Host,
            port = uri.IsDefaultPort ? 443 : uri.Port,
            path = uri.PathAndQuery,
            method = "GET",
            protocol = "https:"
        }, callback);
    }

    public static ClientRequest request(HttpsRequestOptions options, Action<IncomingMessage>? callback = null)
    {
        var normalized = NormalizeHttpsOptions(options);
        return new ClientRequest(CreateHttpClient(normalized), normalized, callback, ownsHttpClient: true);
    }

    public static ClientRequest get(string url, Action<IncomingMessage>? callback = null)
    {
        var req = request(url, callback);
        _ = req.end();
        return req;
    }

    public static ClientRequest get(HttpsRequestOptions options, Action<IncomingMessage>? callback = null)
    {
        var req = request(options, callback);
        _ = req.end();
        return req;
    }

    private static HttpsRequestOptions NormalizeHttpsOptions(HttpsRequestOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        options.protocol = "https:";
        if (options.port == 80)
            options.port = 443;

        return options;
    }

    private static HttpClient CreateHttpClient(HttpsRequestOptions options)
    {
        var context = tls.createSecureContext(new SecureContextOptions
        {
            ca = options.ca,
            cert = options.cert,
            key = options.key,
            pfx = options.pfx,
            passphrase = options.passphrase,
            minVersion = options.minVersion,
            maxVersion = options.maxVersion,
        });
        var handler = new HttpClientHandler
        {
            SslProtocols = context.Protocols,
        };
        if (context.Certificate != null)
        {
            handler.ClientCertificates.Add(context.Certificate);
        }
        if (options.rejectUnauthorized == false)
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }
        else if (context.CACertificates is { Count: > 0 } roots)
        {
            handler.ServerCertificateCustomValidationCallback = (_, certificate, _, errors) =>
                ValidateWithCustomRoots(certificate, errors, roots);
        }
        return new HttpClient(handler, disposeHandler: true);
    }

    private static bool ValidateWithCustomRoots(
        X509Certificate2? certificate,
        SslPolicyErrors errors,
        X509Certificate2Collection roots)
    {
        if (certificate == null || (errors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
            return false;

        using var chain = new X509Chain();
        chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        chain.ChainPolicy.CustomTrustStore.AddRange(roots);
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        return chain.Build(certificate);
    }
}

public sealed class HttpsServerOptions : TlsOptions
{
    public int? maxHeaderSize { get; set; }
}

public sealed class HttpsRequestOptions : RequestOptions
{
    public TsValue ca { get; set; } = TsValue.undefined();
    public TsValue cert { get; set; } = TsValue.undefined();
    public TsValue key { get; set; } = TsValue.undefined();
    public TsValue pfx { get; set; } = TsValue.undefined();
    public string? passphrase { get; set; }
    public string? minVersion { get; set; }
    public string? maxVersion { get; set; }
    public bool? rejectUnauthorized { get; set; }
}
