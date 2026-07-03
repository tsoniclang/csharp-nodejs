using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tsonic.CSharp.Node;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node.Http;

/// <summary>
/// Implements Node.js http.Server functionality using Kestrel.
/// Extends EventEmitter to support events like 'request', 'connection', 'close', etc.
/// </summary>
#pragma warning disable ASPDEPR004 // WebHostBuilder deprecation
#pragma warning disable ASPDEPR008 // IWebHost deprecation
public partial class Server : EventEmitter
{
    private IWebHost? _host;
    private AddressInfo? _boundAddress;
    private readonly Action<IncomingMessage, ServerResponse>? _requestListener;
    private int _maxHeadersCount = 2000;
    private int _timeout = 0; // 0 means no timeout (Node.js default)
    private int _headersTimeout = 60000; // 60 seconds (Node.js default)
    private int _requestTimeout = 300000; // 300 seconds (5 minutes, Node.js default)
    private int _keepAliveTimeout = 5000; // 5 seconds (Node.js default)
    private bool _listening = false;

    private static IPAddress ResolveHostname(string hostname)
    {
        if (IPAddress.TryParse(hostname, out var parsed))
        {
            return parsed;
        }

        if (string.Equals(hostname, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            return IPAddress.Loopback;
        }

        var addresses = Dns.GetHostAddresses(hostname);
        if (addresses.Length == 0)
        {
            throw new InvalidOperationException($"Unable to resolve hostname: {hostname}");
        }

        return addresses[0];
    }

    /// <summary>
    /// Creates a new HTTP server.
    /// </summary>
    /// <param name="requestListener">Optional request handler function.</param>
    public Server(Action<IncomingMessage, ServerResponse>? requestListener = null)
    {
        _requestListener = requestListener;

        // If request listener provided, register it as event listener
        if (requestListener != null)
        {
            on("request", requestListener);
        }
    }

    /// <summary>
    /// Limits maximum incoming headers count.
    /// If set to 0, no limit will be applied.
    /// </summary>
    public int maxHeadersCount
    {
        get => _maxHeadersCount;
        set => _maxHeadersCount = value;
    }

    /// <summary>
    /// Sets the timeout value in milliseconds for receiving the entire request from the client.
    /// Default: 0 (no timeout)
    /// </summary>
    public int timeout
    {
        get => _timeout;
        set => _timeout = JsNumeric.RequireNonNegativeInt(value, nameof(timeout));
    }

    /// <summary>
    /// Limits the amount of time the parser will wait to receive the complete HTTP headers.
    /// Default: 60000 (60 seconds)
    /// </summary>
    public int headersTimeout
    {
        get => _headersTimeout;
        set => _headersTimeout = JsNumeric.RequireNonNegativeInt(value, nameof(headersTimeout));
    }

    /// <summary>
    /// Sets the timeout value in milliseconds for receiving the entire request from the client.
    /// Default: 300000 (5 minutes)
    /// </summary>
    public int requestTimeout
    {
        get => _requestTimeout;
        set => _requestTimeout = JsNumeric.RequireNonNegativeInt(value, nameof(requestTimeout));
    }

    /// <summary>
    /// The number of milliseconds of inactivity a server needs to wait for additional data after it has finished writing the last response,
    /// before a socket will be destroyed.
    /// Default: 5000 (5 seconds)
    /// </summary>
    public int keepAliveTimeout
    {
        get => _keepAliveTimeout;
        set => _keepAliveTimeout = JsNumeric.RequireNonNegativeInt(value, nameof(keepAliveTimeout));
    }

    /// <summary>
    /// Indicates whether or not the server is listening for connections.
    /// </summary>
    public bool listening => _listening;

    /// <summary>
    /// Begin accepting connections on the specified port and hostname.
    /// </summary>
    /// <param name="port">The port number.</param>
    /// <param name="hostname">The hostname. Default: all interfaces</param>
    /// <param name="backlog">Maximum length of the queue of pending connections (ignored in Kestrel).</param>
    /// <param name="callback">Optional callback when server has been started.</param>
    /// <returns>The server instance for chaining.</returns>
    public Server listen(int port, string? hostname = null, int? backlog = null, Action? callback = null)
    {
        if (_listening)
        {
            throw new InvalidOperationException("Server is already listening");
        }

        var normalizedPort = JsNumeric.RequirePort(port, nameof(port));
        var resolvedHostname = string.IsNullOrEmpty(hostname) ? null : ResolveHostname(hostname);
        var listenPort = normalizedPort == 0
            ? ReserveEphemeralPort(resolvedHostname ?? IPAddress.Loopback)
            : normalizedPort;

        if (backlog.HasValue)
        {
            JsNumeric.RequireNonNegativeInt(backlog.Value, nameof(backlog));
        }

        // Use minimal WebHost setup to avoid file watchers
        var host = new WebHostBuilder()
            .UseKestrel(options =>
            {
                if (string.IsNullOrEmpty(hostname))
                {
                    // Listen on all interfaces
                    options.ListenAnyIP(listenPort, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1;
                    });
                }
                else
                {
                    // Listen on specific hostname
                    options.Listen(resolvedHostname!, listenPort, listenOptions =>
                    {
                        listenOptions.Protocols = HttpProtocols.Http1;
                    });
                }

                // Configure limits
                options.Limits.MaxRequestHeaderCount = _maxHeadersCount;
                options.Limits.MaxRequestHeadersTotalSize = http.maxHeaderSize;
                options.Limits.KeepAliveTimeout = TimeSpan.FromMilliseconds(_keepAliveTimeout);
                options.Limits.RequestHeadersTimeout = TimeSpan.FromMilliseconds(_headersTimeout);
            })
            .SuppressStatusMessages(true)
            .Configure(app =>
            {
                // Main request handler - use async to properly await response writes
                app.Run(async context =>
                {
                    var req = new IncomingMessage(context.Request);
                    var res = new ServerResponse(context.Response);

                    // Emit 'request' event - registered listeners will handle the request
                    emit("request", req, res);

                    await context.Response.CompleteAsync();
                });
            })
            .Build();

        _host = host;
        ProcessKeepAlive.Acquire();

        try
        {
            _host.Start();
            _listening = true;
            _boundAddress = ResolveBoundAddress() ?? new AddressInfo
            {
                address = resolvedHostname?.ToString() ?? IPAddress.Loopback.ToString(),
                family = (resolvedHostname ?? IPAddress.Loopback).AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6",
                port = listenPort
            };
            emit("listening");
            callback?.Invoke();
            return this;
        }
        catch
        {
            _host.Dispose();
            _host = null;
            ProcessKeepAlive.Release();
            throw;
        }
    }

    /// <summary>
    /// Begin accepting connections on the specified port.
    /// </summary>
    /// <param name="port">The port number.</param>
    /// <param name="callback">Optional callback when server has been started.</param>
    /// <returns>The server instance for chaining.</returns>
    public Server listen(int port, Action? callback)
    {
        return listen(port, null, null, callback);
    }

    /// <summary>
    /// Begin accepting connections on the specified port and hostname.
    /// </summary>
    public Server listen(int port, string hostname, Action? callback)
    {
        return listen(port, hostname, null, callback);
    }

    /// <summary>
    /// Stops the server from accepting new connections.
    /// </summary>
    /// <param name="callback">Optional callback when server has closed.</param>
    /// <returns>The server instance for chaining.</returns>
    public Server close(Action? callback = null)
    {
        if (_host == null)
        {
            callback?.Invoke();
            return this;
        }

        try
        {
            using var shutdownCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            _host.StopAsync(shutdownCts.Token).GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            // Fall back to dispose below if graceful shutdown takes too long.
        }
        finally
        {
            _host.Dispose();
            _boundAddress = null;
            _host = null;
            _listening = false;
            ProcessKeepAlive.Release();
            emit("close");
            callback?.Invoke();
        }

        return this;
    }

    /// <summary>
    /// Sets the timeout value for sockets and emits a 'timeout' event on the Server object.
    /// </summary>
    /// <param name="msecs">Timeout in milliseconds.</param>
    /// <param name="callback">Optional callback to be added as a listener on the 'timeout' event.</param>
    /// <returns>The server instance for chaining.</returns>
    public Server setTimeout(int msecs, Action? callback = null)
    {
        _timeout = JsNumeric.RequireNonNegativeInt(msecs, nameof(msecs));

        if (callback != null)
        {
            on("timeout", callback);
        }

        return this;
    }

    /// <summary>
    /// Returns the bound address, the address family name, and port of the server.
    /// Only useful after 'listening' event.
    /// </summary>
    /// <returns>An object with 'port', 'family', and 'address' properties.</returns>
    public AddressInfo? address()
    {
        return _boundAddress;
    }

    private AddressInfo? ResolveBoundAddress()
    {
        var boundAddress =
            _host?.ServerFeatures.Get<IServerAddressesFeature>()?.Addresses?.FirstOrDefault()
            ?? _host?.Services.GetService<IServer>()?.Features.Get<IServerAddressesFeature>()?.Addresses?.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(boundAddress))
        {
            return null;
        }

        if (!Uri.TryCreate(boundAddress, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var host = uri.Host;
        if (string.IsNullOrEmpty(host))
        {
            host = "0.0.0.0";
        }

        var family = IPAddress.TryParse(host, out var parsed)
            ? parsed.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6"
            : "IPv4";

        return new AddressInfo
        {
            address = host,
            family = family,
            port = uri.Port
        };
    }

    private static int ReserveEphemeralPort(IPAddress address)
    {
        var listener = new TcpListener(address, 0);
        listener.Start();
        try
        {
            return ((IPEndPoint)listener.LocalEndpoint).Port;
        }
        finally
        {
            listener.Stop();
        }
    }
}

/// <summary>
/// Information about a server's bound address.
/// </summary>
public class AddressInfo
{
    /// <summary>
    /// The port number the server is listening on.
    /// </summary>
    public int port { get; set; }

    /// <summary>
    /// The address family (e.g., "IPv4" or "IPv6").
    /// </summary>
    public string family { get; set; } = "IPv4";

    /// <summary>
    /// The IP address the server is listening on.
    /// </summary>
    public string address { get; set; } = "";
}
