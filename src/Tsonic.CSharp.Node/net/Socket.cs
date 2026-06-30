using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// This class is an abstraction of a TCP socket or a streaming IPC endpoint.
/// It is also an EventEmitter.
/// </summary>
public partial class Socket : Stream
{
    private TcpClient? _client;
    private NetworkStream? _stream;
    private bool _connecting = false;
    private bool _destroyed = false;
    private bool _reading = false;
    private bool _paused = false;
    private int _timeout = 0;
    private bool _allowHalfOpen = false;

    // Write queue for FIFO ordering (like Node.js)
    private readonly BlockingCollection<WriteRequest> _writeQueue = new BlockingCollection<WriteRequest>();
    private Task? _writeLoopTask;
    private bool _writeLoopStarted = false;
    private readonly object _writeLoopLock = new object();
    private readonly ManualResetEventSlim _writeQueueEmpty = new ManualResetEventSlim(true);

    private record WriteRequest(byte[] Data, Action<Exception?>? Callback);

    /// <summary>
    /// The amount of received bytes.
    /// </summary>
    public long bytesRead { get; private set; }

    /// <summary>
    /// The amount of bytes sent.
    /// </summary>
    public long bytesWritten { get; private set; }

    /// <summary>
    /// Whether the connection is active.
    /// </summary>
    public bool connecting => _connecting;

    /// <summary>
    /// Whether the socket has been destroyed.
    /// </summary>
    public bool destroyed => _destroyed;

    /// <summary>
    /// The string representation of the local IP address.
    /// </summary>
    public string? localAddress { get; private set; }

    /// <summary>
    /// The numeric representation of the local port.
    /// </summary>
    public int? localPort { get; private set; }

    /// <summary>
    /// The string representation of the local IP family.
    /// </summary>
    public string? localFamily { get; private set; }

    /// <summary>
    /// The string representation of the remote IP address.
    /// </summary>
    public string? remoteAddress { get; private set; }

    /// <summary>
    /// The numeric representation of the remote port.
    /// </summary>
    public int? remotePort { get; private set; }

    /// <summary>
    /// The string representation of the remote IP family.
    /// </summary>
    public string? remoteFamily { get; private set; }

    /// <summary>
    /// This property represents the state of the connection as a string.
    /// </summary>
    public string readyState
    {
        get
        {
            if (_destroyed) return "closed";
            if (_connecting) return "opening";
            if (_client?.Connected == true) return "open";
            return "closed";
        }
    }

    /// <summary>
    /// Creates a new Socket instance.
    /// </summary>
    public Socket() : this((SocketConstructorOpts?)null)
    {
    }

    /// <summary>
    /// Creates a new Socket instance with options.
    /// </summary>
    /// <param name="options">Socket constructor options</param>
    public Socket(SocketConstructorOpts? options) : base()
    {
        _allowHalfOpen = options?.allowHalfOpen ?? false;
        // Note: fd, readable, writable options not fully supported in .NET
    }

    internal Socket(TcpClient client) : base()
    {
        _client = client;
        _stream = client.GetStream();
        UpdateAddressInfo();
        // Don't start reading immediately - let the connection callback register handlers first
        // StartReading will be called after emitting the connection event
    }

    /// <summary>
    /// Initiate a connection on a given socket.
    /// </summary>
    /// <param name="port">Port to connect to</param>
    /// <param name="host">Host to connect to</param>
    /// <param name="connectionListener">Connection listener callback</param>
    /// <returns>The socket itself</returns>
    public Socket connect(int port, string? host = null, Action? connectionListener = null)
    {
        if (connectionListener != null)
        {
            once("connect", connectionListener);
        }

        _connecting = true;
        var hostname = host ?? "localhost";

        BackgroundDispatch.RunAsync(async () =>
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(hostname, port);
                _stream = _client.GetStream();
                _connecting = false;
                UpdateAddressInfo();
                emit("connect");
                emit("ready");
                StartReading();
            }
            catch (Exception ex)
            {
                _connecting = false;
                emit("error", ex);
            }
        });

        return this;
    }

    /// <summary>
    /// Initiate a connection with options.
    /// </summary>
    /// <param name="options">Connection options</param>
    /// <param name="connectionListener">Connection listener callback</param>
    /// <returns>The socket itself</returns>
    public Socket connect(TcpSocketConnectOpts options, Action? connectionListener = null)
    {
        return connect(options.port, options.host, connectionListener);
    }

    /// <summary>
    /// Initiate a connection using a path (IPC).
    /// </summary>
    /// <param name="path">Path to connect to</param>
    /// <param name="connectionListener">Connection listener callback</param>
    /// <returns>The socket itself</returns>
    public Socket connect(string path, Action? connectionListener = null)
    {
        // IPC connections not fully supported in .NET cross-platform
        throw new NotSupportedException("IPC connections via path not supported");
    }

}

#pragma warning restore CS8981
#pragma warning restore IDE1006
