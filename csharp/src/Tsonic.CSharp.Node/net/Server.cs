using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// This class is used to create a TCP or IPC server.
/// </summary>
public class Server : EventEmitter
{
    private TcpListener? _listener;
    private bool _listening = false;
    private bool _allowHalfOpen = false;
    private bool _pauseOnConnect = false;
    private int _maxConnections = 0;
    private int _connections = 0;

    /// <summary>
    /// Set to true when the server is listening for connections.
    /// </summary>
    public bool listening => _listening;

    /// <summary>
    /// The maximum number of connections.
    /// </summary>
    public int maxConnections
    {
        get => _maxConnections;
        set => _maxConnections = value;
    }

    /// <summary>
    /// Creates a new Server instance.
    /// </summary>
    public Server() : this(null, null)
    {
    }

    /// <summary>
    /// Creates a new Server instance with a connection listener.
    /// </summary>
    /// <param name="connectionListener">Connection listener callback</param>
    public Server(Action<Socket>? connectionListener) : this(null, connectionListener)
    {
    }

    /// <summary>
    /// Creates a new Server instance with options and a connection listener.
    /// </summary>
    /// <param name="options">Server options</param>
    /// <param name="connectionListener">Connection listener callback</param>
    public Server(ServerOpts? options, Action<Socket>? connectionListener) : base()
    {
        _allowHalfOpen = options?.allowHalfOpen ?? false;
        _pauseOnConnect = options?.pauseOnConnect ?? false;

        if (connectionListener != null)
        {
            on("connection", connectionListener);
        }
    }

    /// <summary>
    /// Start a server listening for connections.
    /// </summary>
    /// <param name="port">Port to listen on</param>
    /// <param name="hostname">Host name to listen on</param>
    /// <param name="backlog">Backlog queue size</param>
    /// <param name="listeningListener">Listening callback</param>
    /// <returns>The server itself</returns>
    public Server listen(int port, string hostname, int backlog, Action? listeningListener = null)
    {
        return ListenInternal(port, hostname, backlog, listeningListener);
    }

    /// <summary>
    /// Start a server listening for connections.
    /// </summary>
    public Server listen(int port, string hostname, Action? listeningListener = null)
    {
        return ListenInternal(port, hostname, 511, listeningListener);
    }

    /// <summary>
    /// Start a server listening for connections.
    /// </summary>
    public Server listen(int port, int backlog, Action? listeningListener = null)
    {
        return ListenInternal(port, null, backlog, listeningListener);
    }

    /// <summary>
    /// Start a server listening for connections.
    /// </summary>
    public Server listen(int port, Action? listeningListener = null)
    {
        return ListenInternal(port, null, 511, listeningListener);
    }

    private Server ListenInternal(int port, string? hostname, int backlog, Action? listeningListener)
    {
        if (_listening)
        {
            throw new InvalidOperationException("Server is already listening");
        }

        if (listeningListener != null)
        {
            once("listening", listeningListener);
        }

        var host = hostname ?? "0.0.0.0";
        var ipAddress = IPAddress.Parse(host == "localhost" ? "127.0.0.1" : host);

        _listener = new TcpListener(ipAddress, port);

        try
        {
            _listener.Start(backlog);
            _listening = true;
            emit("listening");
        }
        catch (Exception ex)
        {
            emit("error", ex);
            return this;
        }

        BackgroundDispatch.Run(AcceptConnectionsLoop, "Tsonic.CSharp.Node.Server.AcceptConnections");

        return this;
    }

    private void AcceptConnectionsLoop()
    {
        try
        {
            while (_listening)
            {
                try
                {
                    var client = _listener!.AcceptTcpClient();
                    _connections++;

                    var socket = new Socket(client);

                    if (_maxConnections > 0 && _connections > _maxConnections)
                    {
                        socket.destroy();
                        _connections--;
                        emit("drop", new { });
                    }
                    else
                    {
                        emit("connection", socket);
                        socket.StartReading();
                    }
                }
                catch (SocketException)
                {
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            emit("error", ex);
        }
    }

    /// <summary>
    /// Start a server listening with options.
    /// </summary>
    /// <param name="options">Listen options</param>
    /// <param name="listeningListener">Listening callback</param>
    /// <returns>The server itself</returns>
    public Server listen(ListenOptions options, Action? listeningListener = null)
    {
        if (options.port.HasValue)
        {
            return ListenInternal(options.port.Value, options.host, options.backlog ?? 511, listeningListener);
        }
        else if (options.path != null)
        {
            // IPC not fully supported
            throw new NotSupportedException("IPC server not supported");
        }
        else
        {
            throw new ArgumentException("Either port or path must be specified");
        }
    }

    /// <summary>
    /// Stops the server from accepting new connections.
    /// </summary>
    /// <param name="callback">Callback when server is closed</param>
    /// <returns>The server itself</returns>
    public Server close(Action<Exception?>? callback = null)
    {
        if (!_listening)
        {
            var error = new InvalidOperationException("Server is not listening");
            callback?.Invoke(error);
            return this;
        }

        _listening = false;
        _listener?.Stop();

        if (callback != null)
        {
            once("close", () => callback(null));
        }

        emit("close");

        return this;
    }

    /// <summary>
    /// Returns the bound address, the address family name, and port of the server.
    /// </summary>
    /// <returns>Address info or null</returns>
    public object? address()
    {
        if (_listener?.LocalEndpoint is IPEndPoint endPoint)
        {
            return new AddressInfo
            {
                address = endPoint.Address.ToString(),
                family = endPoint.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6",
                port = endPoint.Port
            };
        }
        return null;
    }

    /// <summary>
    /// Asynchronously get the number of concurrent connections on the server.
    /// </summary>
    /// <param name="callback">Callback with connection count</param>
    public void getConnections(Action<Exception?, int> callback)
    {
        callback(null, _connections);
    }

    /// <summary>
    /// Calling unref() on a server will allow the program to exit if this is the only active server.
    /// </summary>
    /// <returns>The server itself</returns>
    public Server unref()
    {
        // Not applicable in .NET managed context
        return this;
    }

    /// <summary>
    /// Opposite of unref().
    /// </summary>
    /// <returns>The server itself</returns>
    public Server @ref()
    {
        // Not applicable in .NET managed context
        return this;
    }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
