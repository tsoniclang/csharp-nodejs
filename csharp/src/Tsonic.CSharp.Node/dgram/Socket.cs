using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Encapsulates the datagram functionality. UDP socket for sending and receiving datagrams.
/// </summary>
public partial class DgramSocket : EventEmitter
{
    private UdpClient? _socket;
    private readonly string _type;
    private readonly SocketOptions _options;
    private bool _isBound = false;
    private bool _isClosed = false;
    private bool _isConnected = false;
    private IPEndPoint? _remoteEndPoint;
    private Thread? _receiveThread;

    internal DgramSocket(string type, Action<byte[], RemoteInfo>? callback = null)
    {
        _type = type;
        _options = new SocketOptions { type = type };

        if (callback != null)
        {
            on("message", (Action<byte[], RemoteInfo>)callback);
        }
    }

    internal DgramSocket(SocketOptions options, Action<byte[], RemoteInfo>? callback = null)
    {
        _type = options.type;
        _options = options;

        if (callback != null)
        {
            on("message", (Action<byte[], RemoteInfo>)callback);
        }
    }

    /// <summary>
    /// Returns an object containing the address information for a socket.
    /// </summary>
    /// <returns>Address information</returns>
    public AddressInfo address()
    {
        if (!_isBound || _socket == null)
        {
            throw new InvalidOperationException("Socket is not bound");
        }

        var localEP = (IPEndPoint)_socket.Client.LocalEndPoint!;
        return new AddressInfo
        {
            address = localEP.Address.ToString(),
            family = localEP.AddressFamily == AddressFamily.InterNetworkV6 ? "IPv6" : "IPv4",
            port = localEP.Port
        };
    }

    /// <summary>
    /// Causes the socket to listen for datagram messages on a named port and optional address.
    /// </summary>
    /// <param name="port">Port number (0 for random port)</param>
    /// <param name="address">Address to bind to</param>
    /// <param name="callback">Callback when binding is complete</param>
    public DgramSocket bind(int port = 0, string? address = null, Action? callback = null)
    {
        if (_isBound)
        {
            throw new InvalidOperationException("Socket is already bound");
        }

        try
        {
            var addressFamily = _type == "udp6" ? AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
            _socket = new UdpClient(addressFamily);

            // Apply socket options
            if (_options.reuseAddr)
            {
                _socket.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            }

            if (_options.recvBufferSize.HasValue)
            {
                _socket.Client.ReceiveBufferSize = _options.recvBufferSize.Value;
            }

            if (_options.sendBufferSize.HasValue)
            {
                _socket.Client.SendBufferSize = _options.sendBufferSize.Value;
            }

            // Bind to address and port
            IPAddress bindAddress;
            if (string.IsNullOrEmpty(address))
            {
                bindAddress = _type == "udp6" ? IPAddress.IPv6Any : IPAddress.Any;
            }
            else
            {
                bindAddress = IPAddress.Parse(address);
            }

            var endPoint = new IPEndPoint(bindAddress, port);
            _socket.Client.Bind(endPoint);
            _isBound = true;

            // Start receiving messages
            StartReceiving();

            // Emit listening event
            emit("listening");
            callback?.Invoke();
        }
        catch (Exception ex)
        {
            emit("error", ex);
        }

        return this;
    }

    /// <summary>
    /// Causes the socket to listen for datagram messages on a named port.
    /// </summary>
    /// <param name="port">Port number (0 for random port)</param>
    /// <param name="callback">Callback when binding is complete</param>
    public DgramSocket bind(int port, Action? callback)
    {
        return bind(port, null, callback);
    }

    /// <summary>
    /// Causes the socket to listen for datagram messages.
    /// </summary>
    /// <param name="callback">Callback when binding is complete</param>
    public DgramSocket bind(Action? callback)
    {
        return bind(0, null, callback);
    }

    /// <summary>
    /// Causes the socket to listen for datagram messages using bind options.
    /// </summary>
    /// <param name="options">Bind options</param>
    /// <param name="callback">Callback when binding is complete</param>
    public DgramSocket bind(BindOptions options, Action? callback = null)
    {
        if (options.fd.HasValue)
        {
            throw new NotSupportedException("File descriptor binding is not supported in .NET");
        }

        var port = options.port ?? 0;
        var address = options.address;

        if (options.exclusive)
        {
            // In .NET, we can set ExclusiveAddressUse before binding
            // Note: This needs to be done before binding, so we'll handle it here
        }

        return bind(port, address, callback);
    }

    /// <summary>
    /// Close the underlying socket and stop listening for data on it.
    /// </summary>
    /// <param name="callback">Called when the socket has been closed</param>
    public DgramSocket close(Action? callback = null)
    {
        if (_isClosed)
        {
            return this;
        }

        _isClosed = true;
        _socket?.Close();
        _socket?.Dispose();
        _socket = null;

        emit("close");
        callback?.Invoke();

        return this;
    }

    /// <summary>
    /// Associates the socket to a remote address and port.
    /// </summary>
    /// <param name="port">Remote port</param>
    /// <param name="address">Remote address</param>
    /// <param name="callback">Called when connection is complete</param>
    public void connect(int port, string? address = null, Action? callback = null)
    {
        if (_isConnected)
        {
            throw new InvalidOperationException("Socket is already connected");
        }

        try
        {
            if (string.IsNullOrEmpty(address))
            {
                address = _type == "udp6" ? "::1" : "127.0.0.1";
            }

            var ipAddress = IPAddress.Parse(address);
            _remoteEndPoint = new IPEndPoint(ipAddress, port);

            // Auto-bind if not already bound
            if (_socket == null)
            {
                bind();
            }

            _socket!.Connect(_remoteEndPoint);
            _isConnected = true;
            emit("connect");
            callback?.Invoke();
        }
        catch (Exception ex)
        {
            emit("error", ex);
            callback?.Invoke();
        }
    }

    /// <summary>
    /// Associates the socket to a remote port on localhost.
    /// </summary>
    /// <param name="port">Remote port</param>
    /// <param name="callback">Called when connection is complete</param>
    public void connect(int port, Action callback)
    {
        connect(port, null, callback);
    }

    /// <summary>
    /// Disassociates a connected socket from its remote address.
    /// </summary>
    public void disconnect()
    {
        if (!_isConnected)
        {
            throw new InvalidOperationException("Socket is not connected");
        }

        _remoteEndPoint = null;
        _isConnected = false;
    }

    private void StartReceiving()
    {
        _receiveThread = new Thread(ReceiveLoop)
        {
            IsBackground = true
        };
        _receiveThread.Start();
    }

    private void ReceiveLoop()
    {
        while (!_isClosed && _socket != null)
        {
            try
            {
                IPEndPoint? remoteEP = null;
                var data = _socket.Receive(ref remoteEP);

                if (data != null && data.Length > 0 && remoteEP != null)
                {
                    var rinfo = new RemoteInfo
                    {
                        address = remoteEP.Address.ToString(),
                        family = remoteEP.AddressFamily == AddressFamily.InterNetworkV6 ? "IPv6" : "IPv4",
                        port = remoteEP.Port,
                        size = data.Length
                    };

                    emit("message", data, rinfo);
                }
            }
            catch (SocketException)
            {
                // Socket closed or error - exit loop
                if (!_isClosed)
                {
                    break;
                }
            }
            catch (Exception ex)
            {
                if (!_isClosed)
                {
                    emit("error", ex);
                }
                break;
            }
        }
    }
}
