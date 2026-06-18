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
public class Socket : Stream
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

    /// <summary>
    /// Sends data on the socket.
    /// </summary>
    /// <param name="data">Data to write</param>
    /// <param name="callback">Callback when write completes</param>
    /// <returns>True if flushed to kernel buffer</returns>
    public bool write(byte[] data, Action<Exception?>? callback = null)
    {
        if (_stream == null || _destroyed)
        {
            callback?.Invoke(new InvalidOperationException("Socket not connected"));
            return false;
        }

        // Queue the write request for FIFO processing (like Node.js)
        _writeQueueEmpty.Reset();
        _writeQueue.Add(new WriteRequest(data, callback));

        // Start the write loop if not already running
        StartWriteLoop();

        return true;
    }

    /// <summary>
    /// Starts the write loop that processes queued writes in FIFO order.
    /// </summary>
    private void StartWriteLoop()
    {
        lock (_writeLoopLock)
        {
            if (_writeLoopStarted) return;
            _writeLoopStarted = true;
        }

        _writeLoopTask = BackgroundDispatch.RunAsync(async () =>
        {
            try
            {
                foreach (var request in _writeQueue.GetConsumingEnumerable())
                {
                    if (_destroyed || _stream == null) break;

                    try
                    {
                        await _stream.WriteAsync(request.Data, 0, request.Data.Length);
                        bytesWritten += request.Data.Length;
                        request.Callback?.Invoke(null);
                        emit("drain");
                    }
                    catch (Exception ex)
                    {
                        request.Callback?.Invoke(ex);
                        emit("error", ex);
                    }

                    // Signal if queue is empty
                    if (_writeQueue.Count == 0)
                    {
                        _writeQueueEmpty.Set();
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // Queue was completed, this is expected during shutdown
            }
            finally
            {
                _writeQueueEmpty.Set();
            }
        });
    }

    /// <summary>
    /// Sends string data on the socket.
    /// </summary>
    /// <param name="data">String data to write</param>
    /// <param name="encoding">Encoding to use</param>
    /// <param name="callback">Callback when write completes</param>
    /// <returns>True if flushed to kernel buffer</returns>
    public bool write(string data, string? encoding = null, Action<Exception?>? callback = null)
    {
        var enc = encoding == null ? Encoding.UTF8 : Encoding.GetEncoding(encoding);
        return write(enc.GetBytes(data), callback);
    }

    /// <summary>
    /// Half-closes the socket.
    /// </summary>
    /// <param name="callback">Callback when finished</param>
    /// <returns>The socket itself</returns>
    public Socket end(Action? callback = null)
    {
        if (_stream != null && !_destroyed)
        {
            // Wait for pending writes to complete before closing (like Node.js)
            BackgroundDispatch.Run(() =>
            {
                // Wait for write queue to be empty with timeout
                _writeQueueEmpty.Wait(TimeSpan.FromSeconds(30));

                // Complete the write queue to stop the write loop
                _writeQueue.CompleteAdding();

                _stream?.Close();
                emit("end");
                callback?.Invoke();
            });
        }
        return this;
    }

    /// <summary>
    /// Half-closes the socket after writing data.
    /// </summary>
    /// <param name="data">Data to write before closing</param>
    /// <param name="callback">Callback when finished</param>
    /// <returns>The socket itself</returns>
    public Socket end(byte[] data, Action? callback = null)
    {
        write(data, (err) =>
        {
            if (err == null)
            {
                end(callback);
            }
        });
        return this;
    }

    /// <summary>
    /// Half-closes the socket after writing string data.
    /// </summary>
    /// <param name="data">String data to write before closing</param>
    /// <param name="encoding">Encoding to use</param>
    /// <param name="callback">Callback when finished</param>
    /// <returns>The socket itself</returns>
    public Socket end(string data, string? encoding = null, Action? callback = null)
    {
        var enc = encoding == null ? Encoding.UTF8 : Encoding.GetEncoding(encoding);
        return end(enc.GetBytes(data), callback);
    }

    /// <summary>
    /// Ensures that no more I/O activity happens on this socket.
    /// </summary>
    /// <param name="error">Optional error</param>
    /// <returns>The socket itself</returns>
    public new Socket destroy(Exception? error = null)
    {
        if (_destroyed) return this;

        _destroyed = true;
        _stream?.Close();
        _client?.Close();

        emit("close", error != null);
        if (error != null)
        {
            emit("error", error);
        }

        return this;
    }

    /// <summary>
    /// Destroys the socket after all data is written.
    /// </summary>
    public void destroySoon()
    {
        end(() => destroy());
    }

    /// <summary>
    /// Close the TCP connection by sending an RST packet.
    /// </summary>
    /// <returns>The socket itself</returns>
    public Socket resetAndDestroy()
    {
        _client?.Client?.Close(0);
        return destroy();
    }

    /// <summary>
    /// Set the encoding for the socket as a Readable Stream.
    /// </summary>
    /// <param name="encoding">Encoding name</param>
    /// <returns>The socket itself</returns>
    public Socket setEncoding(string? encoding = null)
    {
        // Encoding handling would be implemented with full stream support
        return this;
    }

    /// <summary>
    /// Pauses the reading of data.
    /// </summary>
    /// <returns>The socket itself</returns>
    public Socket pause()
    {
        _paused = true;
        return this;
    }

    /// <summary>
    /// Resumes reading after a call to socket.pause().
    /// </summary>
    /// <returns>The socket itself</returns>
    public Socket resume()
    {
        _paused = false;
        return this;
    }

    /// <summary>
    /// Sets the socket to timeout after timeout milliseconds of inactivity.
    /// </summary>
    /// <param name="timeout">Timeout in milliseconds</param>
    /// <param name="callback">Timeout callback</param>
    /// <returns>The socket itself</returns>
    public Socket setTimeout(int timeout, Action? callback = null)
    {
        _timeout = timeout;
        if (callback != null)
        {
            once("timeout", callback);
        }

        if (_stream != null)
        {
            _stream.ReadTimeout = timeout;
            _stream.WriteTimeout = timeout;
        }

        return this;
    }

    /// <summary>
    /// Enable/disable the use of Nagle's algorithm.
    /// </summary>
    /// <param name="noDelay">Disable Nagle's algorithm if true</param>
    /// <returns>The socket itself</returns>
    public Socket setNoDelay(bool noDelay = true)
    {
        if (_client?.Client != null)
        {
            _client.NoDelay = noDelay;
        }
        return this;
    }

    /// <summary>
    /// Enable/disable keep-alive functionality.
    /// </summary>
    /// <param name="enable">Enable keep-alive if true</param>
    /// <param name="initialDelay">Initial delay in milliseconds</param>
    /// <returns>The socket itself</returns>
    public Socket setKeepAlive(bool enable = false, int initialDelay = 0)
    {
        if (_client?.Client != null)
        {
            _client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, enable);
        }
        return this;
    }

    /// <summary>
    /// Returns the bound address, the address family name and port of the socket.
    /// </summary>
    /// <returns>Address info or empty object</returns>
    public object address()
    {
        if (localAddress != null && localPort.HasValue && localFamily != null)
        {
            return new AddressInfo
            {
                address = localAddress,
                family = localFamily,
                port = localPort.Value
            };
        }
        return new { };
    }

    /// <summary>
    /// Calling unref() on a socket will allow the program to exit if this is the only active socket.
    /// </summary>
    /// <returns>The socket itself</returns>
    public Socket unref()
    {
        // Not applicable in .NET managed context
        return this;
    }

    /// <summary>
    /// Opposite of unref().
    /// </summary>
    /// <returns>The socket itself</returns>
    public Socket @ref()
    {
        // Not applicable in .NET managed context
        return this;
    }

    /// <summary>
    /// Starts the asynchronous read loop to emit "data" events.
    /// Called internally after the connection event is emitted to allow handlers to be registered first.
    /// </summary>
    internal void StartReading()
    {
        if (_reading || _stream == null) return;
        _reading = true;

        BackgroundDispatch.RunAsync(async () =>
        {
            var buffer = new byte[65536]; // 64KB buffer
            try
            {
                while (!_destroyed && _stream != null && _client?.Connected == true)
                {
                    // Wait while paused
                    while (_paused && !_destroyed)
                    {
                        await Task.Delay(10);
                    }

                    if (_destroyed) break;

                    int bytesReadCount;
                    try
                    {
                        bytesReadCount = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    }
                    catch (ObjectDisposedException)
                    {
                        // Stream was closed
                        break;
                    }
                    catch (System.IO.IOException)
                    {
                        // Connection reset or closed
                        break;
                    }

                    if (bytesReadCount == 0)
                    {
                        // End of stream - connection closed by remote
                        emit("end");
                        if (!_allowHalfOpen)
                        {
                            end();
                        }
                        break;
                    }

                    bytesRead += bytesReadCount;

                    // Create a Buffer with the received data
                    var data = new byte[bytesReadCount];
                    System.Array.Copy(buffer, 0, data, 0, bytesReadCount);
                    var nodeBuffer = Buffer.from(data);

                    emit("data", nodeBuffer);
                }
            }
            catch (Exception ex)
            {
                if (!_destroyed)
                {
                    emit("error", ex);
                }
            }
            finally
            {
                _reading = false;
                if (!_destroyed)
                {
                    emit("close", false);
                }
            }
        });
    }

    private void UpdateAddressInfo()
    {
        if (_client?.Client?.LocalEndPoint is IPEndPoint localEP)
        {
            localAddress = localEP.Address.ToString();
            localPort = localEP.Port;
            localFamily = localEP.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6";
        }

        if (_client?.Client?.RemoteEndPoint is IPEndPoint remoteEP)
        {
            remoteAddress = remoteEP.Address.ToString();
            remotePort = remoteEP.Port;
            remoteFamily = remoteEP.AddressFamily == AddressFamily.InterNetwork ? "IPv4" : "IPv6";
        }
    }

    /// <summary>
    /// Gets the underlying TcpClient (for TLS wrapping).
    /// </summary>
    internal TcpClient? GetTcpClient()
    {
        return _client;
    }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
