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
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
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
    private string? _boundPath;
    private readonly Action<IncomingMessage, ServerResponse>? _requestListener;
    private readonly Action<Microsoft.AspNetCore.Server.Kestrel.Core.ListenOptions>? _configureListener;
    private int _maxHeadersCount = 2000;
    private int _timeout = 0; // 0 means no timeout (Node.js default)
    private int _headersTimeout = 60000; // 60 seconds (Node.js default)
    private int _requestTimeout = 300000; // 300 seconds (5 minutes, Node.js default)
    private int _keepAliveTimeout = 5000; // 5 seconds (Node.js default)
    private bool _listening = false;
    private int _referenced;

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
        : this(requestListener, null)
    {
    }

    internal Server(
        Action<IncomingMessage, ServerResponse>? requestListener,
        Action<Microsoft.AspNetCore.Server.Kestrel.Core.ListenOptions>? configureListener)
    {
        _requestListener = requestListener;
        _configureListener = configureListener;

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
    /// <param name="backlog">Maximum length of the queue of pending connections.</param>
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
        if (backlog.HasValue)
        {
            JsNumeric.RequireNonNegativeInt(backlog.Value, nameof(backlog));
        }

        return StartHost(CreateHost(options =>
        {
            if (resolvedHostname is null)
            {
                options.ListenAnyIP(normalizedPort, ConfigureListenOptions);
                return;
            }

            options.Listen(resolvedHostname, normalizedPort, ConfigureListenOptions);
        }, backlog), null, callback);
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

    /// <summary>Begins accepting connections on a Unix-domain socket.</summary>
    public Server listen(string path, Action? callback = null)
    {
        if (_listening)
            throw new InvalidOperationException("Server is already listening");
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Socket path must not be empty.", nameof(path));
        if (OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException("Unix-domain HTTP listeners are not supported on Windows.");

        var absolutePath = Path.GetFullPath(path);
        return StartHost(CreateHost(options =>
            options.ListenUnixSocket(absolutePath, listenOptions =>
                ConfigureListenOptions(listenOptions))), absolutePath, callback);
    }

    /// <summary>
    /// Stops the server from accepting new connections.
    /// </summary>
    /// <param name="callback">Optional callback when server has closed.</param>
    /// <returns>The server instance for chaining.</returns>
    public Server close(Action<Exception?>? callback = null)
    {
        var host = _host;
        if (host == null)
        {
            if (callback is not null)
            {
                var error = new InvalidOperationException("Server is not listening");
                Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => callback(error));
            }
            return this;
        }

        _host = null;
        _boundAddress = null;
        _boundPath = null;
        _listening = false;
        var releaseServerReference = Interlocked.Exchange(ref _referenced, 0) != 0;
        _ = CloseAsync(host, callback, releaseServerReference);

        return this;
    }

    /// <summary>
    /// Allows the process to exit when this server is the only active handle.
    /// </summary>
    /// <returns>The server instance for chaining.</returns>
    public Server unref()
    {
        if (Interlocked.Exchange(ref _referenced, 0) != 0)
            ProcessKeepAlive.Release();
        return this;
    }

    /// <summary>
    /// Restores this listening server as a process-liveness handle.
    /// </summary>
    /// <returns>The server instance for chaining.</returns>
    public Server @ref()
    {
        if (_listening && Interlocked.Exchange(ref _referenced, 1) == 0)
            ProcessKeepAlive.Acquire();
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
    public ServerAddress? address()
    {
        if (_boundPath is not null)
            return new ServerAddress { path = _boundPath };
        return _boundAddress is null ? null : new ServerAddress { address = _boundAddress };
    }

    private IWebHost CreateHost(Action<KestrelServerOptions> configureEndpoint, int? backlog = null)
    {
        return new WebHostBuilder()
            .UseKestrel(options =>
            {
                configureEndpoint(options);
                options.Limits.MaxRequestHeaderCount = _maxHeadersCount;
                options.Limits.MaxRequestHeadersTotalSize = http.maxHeaderSize;
                options.Limits.MaxRequestBodySize = null;
                options.Limits.KeepAliveTimeout = TimeSpan.FromMilliseconds(_keepAliveTimeout);
                options.Limits.RequestHeadersTimeout = TimeSpan.FromMilliseconds(_headersTimeout);
            })
            .ConfigureServices(services =>
            {
                if (backlog.HasValue)
                    services.Configure<SocketTransportOptions>(options => options.Backlog = backlog.Value);
            })
            .SuppressStatusMessages(true)
            .Configure(app => app.Run(HandleRequestAsync))
            .Build();
    }

    private async Task HandleRequestAsync(HttpContext context)
    {
        var request = new IncomingMessage(context.Request);
        var response = new ServerResponse(context.Response);
        JsEventLoop.EnqueueHandleOwned(() => emit("request", request, response));

        try
        {
            await response.Completion.WaitAsync(context.RequestAborted).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
        }
        finally
        {
            request.CloseAfterResponse();
        }
    }

    private Server StartHost(IWebHost host, string? boundPath, Action? callback)
    {
        _host = host;
        try
        {
            host.Start();
            _listening = true;
            _boundPath = boundPath;
            _boundAddress = boundPath is null
                ? ResolveBoundAddress()
                    ?? throw new InvalidOperationException("Kestrel did not publish its bound address.")
                : null;
            if (Interlocked.Exchange(ref _referenced, 1) == 0)
                ProcessKeepAlive.Acquire();
            JsEventLoop.EnqueueReferenced(() =>
            {
                emit("listening");
                callback?.Invoke();
            });
        }
        catch (Exception error)
        {
            host.Dispose();
            _host = null;
            _boundAddress = null;
            _boundPath = null;
            _listening = false;
            if (Interlocked.Exchange(ref _referenced, 0) != 0)
                ProcessKeepAlive.Release();
            JsEventLoop.EnqueueReferenced(() => emit("error", error));
        }
        return this;
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

    private void ConfigureListenOptions(
        Microsoft.AspNetCore.Server.Kestrel.Core.ListenOptions listenOptions)
    {
        listenOptions.Protocols = HttpProtocols.Http1;
        _configureListener?.Invoke(listenOptions);
    }

    private async Task CloseAsync(
        IWebHost host,
        Action<Exception?>? callback,
        bool releaseServerReference)
    {
        Exception? failure = null;
        try
        {
            using var shutdownCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await host.StopAsync(shutdownCts.Token).ConfigureAwait(false);
        }
        catch (Exception error)
        {
            failure = error;
        }
        finally
        {
            host.Dispose();
        }

        Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() =>
        {
            if (failure is not null && callback is null)
                emit("error", failure);
            emit("close");
            callback?.Invoke(failure);
        });
        if (releaseServerReference)
            ProcessKeepAlive.Release();
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

/// <summary>Closed native result for a TCP or Unix-domain listener.</summary>
public sealed class ServerAddress
{
    /// <summary>The bound TCP address, when applicable.</summary>
    public AddressInfo? address { get; init; }
    /// <summary>The bound Unix-domain path, when applicable.</summary>
    public string? path { get; init; }
    /// <summary>The bound TCP port, when applicable.</summary>
    public int? port => address?.port;
}
