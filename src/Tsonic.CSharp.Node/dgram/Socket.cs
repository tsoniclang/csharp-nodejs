using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Encapsulates the datagram functionality. UDP socket for sending and receiving datagrams.
/// </summary>
public class DgramSocket : EventEmitter
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

    /// <summary>
    /// Broadcasts a datagram on the socket.
    /// </summary>
    /// <param name="msg">Message to be sent</param>
    /// <param name="port">Destination port</param>
    /// <param name="address">Destination address</param>
    /// <param name="callback">Called when message has been sent</param>
    public void send(byte[] msg, int? port = null, string? address = null, Action<Exception?, int>? callback = null)
    {
        try
        {
            if (_socket == null)
            {
                // Auto-bind if not bound
                bind();
            }

            int bytesSent;

            if (_isConnected)
            {
                // Send to connected endpoint
                bytesSent = _socket!.Send(msg, msg.Length);
            }
            else
            {
                // Send to specified endpoint
                if (!port.HasValue)
                {
                    throw new ArgumentException("Port must be specified for unconnected socket");
                }

                if (string.IsNullOrEmpty(address))
                {
                    address = _type == "udp6" ? "::1" : "127.0.0.1";
                }

                var ipAddress = IPAddress.Parse(address);
                var endPoint = new IPEndPoint(ipAddress, port.Value);
                bytesSent = _socket!.Send(msg, msg.Length, endPoint);
            }

            callback?.Invoke(null, bytesSent);
        }
        catch (Exception ex)
        {
            emit("error", ex);
            callback?.Invoke(ex, 0);
        }
    }

    /// <summary>
    /// Broadcasts a datagram on the socket with a string message.
    /// </summary>
    /// <param name="msg">String message to be sent</param>
    /// <param name="port">Destination port</param>
    /// <param name="address">Destination address</param>
    /// <param name="callback">Called when message has been sent</param>
    public void send(string msg, int? port = null, string? address = null, Action<Exception?, int>? callback = null)
    {
        var bytes = Encoding.UTF8.GetBytes(msg);
        send(bytes, port, address, callback);
    }

    /// <summary>
    /// Broadcasts a datagram on the socket to a specified port.
    /// </summary>
    /// <param name="msg">Message to be sent</param>
    /// <param name="port">Destination port</param>
    /// <param name="callback">Called when message has been sent</param>
    public void send(byte[] msg, int port, Action<Exception?, int>? callback)
    {
        send(msg, port, null, callback);
    }

    /// <summary>
    /// Broadcasts a datagram on the socket to a specified port with a string message.
    /// </summary>
    /// <param name="msg">String message to be sent</param>
    /// <param name="port">Destination port</param>
    /// <param name="callback">Called when message has been sent</param>
    public void send(string msg, int port, Action<Exception?, int>? callback)
    {
        send(msg, port, null, callback);
    }

    /// <summary>
    /// Broadcasts a datagram on the socket (must be connected).
    /// </summary>
    /// <param name="msg">Message to be sent</param>
    /// <param name="callback">Called when message has been sent</param>
    public void send(byte[] msg, Action<Exception?, int>? callback)
    {
        send(msg, null, null, callback);
    }

    /// <summary>
    /// Broadcasts a datagram on the socket with a string message (must be connected).
    /// </summary>
    /// <param name="msg">String message to be sent</param>
    /// <param name="callback">Called when message has been sent</param>
    public void send(string msg, Action<Exception?, int>? callback)
    {
        send(msg, null, null, callback);
    }

    /// <summary>
    /// Broadcasts a datagram on the socket with offset and length.
    /// </summary>
    /// <param name="msg">Buffer containing the message</param>
    /// <param name="offset">Offset in the buffer where the message starts</param>
    /// <param name="length">Number of bytes in the message</param>
    /// <param name="port">Destination port</param>
    /// <param name="address">Destination address</param>
    /// <param name="callback">Called when message has been sent</param>
    public void send(byte[] msg, int offset, int length, int? port = null, string? address = null, Action<Exception?, int>? callback = null)
    {
        if (offset < 0 || offset >= msg.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be within buffer bounds");
        }
        if (length < 0 || offset + length > msg.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Length must be within buffer bounds");
        }

        // Extract the slice
        var slice = new byte[length];
        Array.Copy(msg, offset, slice, 0, length);
        send(slice, port, address, callback);
    }

    /// <summary>
    /// Broadcasts a datagram on the socket with offset and length to a specified port.
    /// </summary>
    /// <param name="msg">Buffer containing the message</param>
    /// <param name="offset">Offset in the buffer where the message starts</param>
    /// <param name="length">Number of bytes in the message</param>
    /// <param name="port">Destination port</param>
    /// <param name="callback">Called when message has been sent</param>
    public void send(byte[] msg, int offset, int length, int port, Action<Exception?, int>? callback)
    {
        send(msg, offset, length, port, null, callback);
    }

    /// <summary>
    /// Broadcasts a datagram on the socket with offset and length (must be connected).
    /// </summary>
    /// <param name="msg">Buffer containing the message</param>
    /// <param name="offset">Offset in the buffer where the message starts</param>
    /// <param name="length">Number of bytes in the message</param>
    /// <param name="callback">Called when message has been sent</param>
    public void send(byte[] msg, int offset, int length, Action<Exception?, int>? callback)
    {
        send(msg, offset, length, null, null, callback);
    }

    /// <summary>
    /// Sets or clears the SO_BROADCAST socket option.
    /// </summary>
    /// <param name="flag">Enable or disable broadcast</param>
    public void setBroadcast(bool flag)
    {
        if (_socket == null || !_isBound)
        {
            throw new InvalidOperationException("Socket is not bound");
        }

        _socket.EnableBroadcast = flag;
    }

    /// <summary>
    /// Sets the IP_MULTICAST_TTL socket option.
    /// </summary>
    /// <param name="ttl">TTL value (0-255)</param>
    public int setMulticastTTL(int ttl)
    {
        if (_socket == null || !_isBound)
        {
            throw new InvalidOperationException("Socket is not bound");
        }

        if (ttl < 0 || ttl > 255)
        {
            throw new ArgumentException("TTL must be between 0 and 255");
        }

        _socket.Client.SetSocketOption(
            SocketOptionLevel.IP,
            SocketOptionName.MulticastTimeToLive,
            ttl
        );

        return ttl;
    }

    /// <summary>
    /// Sets or clears the IP_MULTICAST_LOOP socket option.
    /// </summary>
    /// <param name="flag">Enable or disable multicast loopback</param>
    public bool setMulticastLoopback(bool flag)
    {
        if (_socket == null || !_isBound)
        {
            throw new InvalidOperationException("Socket is not bound");
        }

        _socket.MulticastLoopback = flag;
        return flag;
    }

    /// <summary>
    /// Tells the kernel to join a multicast group.
    /// </summary>
    /// <param name="multicastAddress">Multicast group address</param>
    /// <param name="multicastInterface">Interface address</param>
    public void addMembership(string multicastAddress, string? multicastInterface = null)
    {
        if (_socket == null)
        {
            // Auto-bind if not bound
            bind();
        }

        var groupAddress = IPAddress.Parse(multicastAddress);

        if (multicastInterface != null)
        {
            var interfaceAddress = IPAddress.Parse(multicastInterface);
            _socket!.JoinMulticastGroup(groupAddress, interfaceAddress);
        }
        else
        {
            _socket!.JoinMulticastGroup(groupAddress);
        }
    }

    /// <summary>
    /// Instructs the kernel to leave a multicast group.
    /// </summary>
    /// <param name="multicastAddress">Multicast group address</param>
    public void dropMembership(string multicastAddress)
    {
        if (_socket == null || !_isBound)
        {
            throw new InvalidOperationException("Socket is not bound");
        }

        var groupAddress = IPAddress.Parse(multicastAddress);
        _socket.DropMulticastGroup(groupAddress);
    }

    /// <summary>
    /// Sets the default outgoing multicast interface of the socket.
    /// </summary>
    /// <param name="multicastInterface">IP address of the multicast interface</param>
    public void setMulticastInterface(string multicastInterface)
    {
        if (_socket == null || !_isBound)
        {
            throw new InvalidOperationException("Socket is not bound");
        }

        var interfaceAddress = IPAddress.Parse(multicastInterface);

        if (_type == "udp4")
        {
            var bytes = interfaceAddress.GetAddressBytes();
            var addressValue = BitConverter.ToInt32(bytes, 0);
            _socket.Client.SetSocketOption(
                SocketOptionLevel.IP,
                SocketOptionName.MulticastInterface,
                addressValue
            );
        }
        else
        {
            var index = BitConverter.ToInt32(interfaceAddress.GetAddressBytes(), 0);
            _socket.Client.SetSocketOption(
                SocketOptionLevel.IPv6,
                SocketOptionName.MulticastInterface,
                index
            );
        }
    }

    /// <summary>
    /// Sets the IP_TTL socket option.
    /// </summary>
    /// <param name="ttl">TTL value (1-255)</param>
    /// <returns>The TTL value</returns>
    public int setTTL(int ttl)
    {
        if (_socket == null || !_isBound)
        {
            throw new InvalidOperationException("Socket is not bound");
        }

        if (ttl < 1 || ttl > 255)
        {
            throw new ArgumentException("TTL must be between 1 and 255");
        }

        _socket.Client.SetSocketOption(
            SocketOptionLevel.IP,
            SocketOptionName.IpTimeToLive,
            ttl
        );

        return ttl;
    }

    /// <summary>
    /// Gets the number of bytes queued for sending.
    /// Note: This is not available in .NET UdpClient, returns 0.
    /// </summary>
    /// <returns>Number of bytes queued (always 0 in .NET)</returns>
    public int getSendQueueSize()
    {
        // Not available in .NET UdpClient
        return 0;
    }

    /// <summary>
    /// Gets the number of send requests currently in the queue.
    /// Note: This is not available in .NET UdpClient, returns 0.
    /// </summary>
    /// <returns>Number of send requests (always 0 in .NET)</returns>
    public int getSendQueueCount()
    {
        // Not available in .NET UdpClient
        return 0;
    }

    /// <summary>
    /// Adds the socket back to reference counting.
    /// Note: Not applicable in .NET, this is a no-op for API compatibility.
    /// </summary>
    /// <returns>This socket instance</returns>
    public DgramSocket @ref()
    {
        // Not applicable in .NET - no-op for compatibility
        return this;
    }

    /// <summary>
    /// Excludes the socket from reference counting.
    /// Note: Not applicable in .NET, this is a no-op for API compatibility.
    /// </summary>
    /// <returns>This socket instance</returns>
    public DgramSocket unref()
    {
        // Not applicable in .NET - no-op for compatibility
        return this;
    }

    /// <summary>
    /// Tells the kernel to join a source-specific multicast channel.
    /// </summary>
    /// <param name="sourceAddress">Source address</param>
    /// <param name="groupAddress">Multicast group address</param>
    /// <param name="multicastInterface">Interface address</param>
    public void addSourceSpecificMembership(string sourceAddress, string groupAddress, string? multicastInterface = null)
    {
        if (_socket == null)
        {
            // Auto-bind if not bound
            bind();
        }

        var source = IPAddress.Parse(sourceAddress);
        var group = IPAddress.Parse(groupAddress);

        // Note: .NET doesn't have direct SSM support, but we can use IP_ADD_SOURCE_MEMBERSHIP socket option
        throw new NotSupportedException("Source-specific multicast is not fully supported in .NET UdpClient");
    }

    /// <summary>
    /// Instructs the kernel to leave a source-specific multicast channel.
    /// </summary>
    /// <param name="sourceAddress">Source address</param>
    /// <param name="groupAddress">Multicast group address</param>
    /// <param name="multicastInterface">Interface address</param>
    public void dropSourceSpecificMembership(string sourceAddress, string groupAddress, string? multicastInterface = null)
    {
        if (_socket == null || !_isBound)
        {
            throw new InvalidOperationException("Socket is not bound");
        }

        // Note: .NET doesn't have direct SSM support
        throw new NotSupportedException("Source-specific multicast is not fully supported in .NET UdpClient");
    }

    /// <summary>
    /// Sets the SO_RCVBUF socket receive buffer size.
    /// </summary>
    /// <param name="size">Buffer size in bytes</param>
    public void setRecvBufferSize(int size)
    {
        if (_socket == null || !_isBound)
        {
            throw new InvalidOperationException("Socket is not bound");
        }

        _socket.Client.ReceiveBufferSize = size;
    }

    /// <summary>
    /// Sets the SO_SNDBUF socket send buffer size.
    /// </summary>
    /// <param name="size">Buffer size in bytes</param>
    public void setSendBufferSize(int size)
    {
        if (_socket == null || !_isBound)
        {
            throw new InvalidOperationException("Socket is not bound");
        }

        _socket.Client.SendBufferSize = size;
    }

    /// <summary>
    /// Gets the SO_RCVBUF socket receive buffer size.
    /// </summary>
    /// <returns>Buffer size in bytes</returns>
    public int getRecvBufferSize()
    {
        if (_socket == null || !_isBound)
        {
            throw new InvalidOperationException("Socket is not bound");
        }

        return _socket.Client.ReceiveBufferSize;
    }

    /// <summary>
    /// Gets the SO_SNDBUF socket send buffer size.
    /// </summary>
    /// <returns>Buffer size in bytes</returns>
    public int getSendBufferSize()
    {
        if (_socket == null || !_isBound)
        {
            throw new InvalidOperationException("Socket is not bound");
        }

        return _socket.Client.SendBufferSize;
    }

    /// <summary>
    /// Returns the remote endpoint information.
    /// </summary>
    /// <returns>Remote address information</returns>
    public AddressInfo remoteAddress()
    {
        if (!_isConnected || _remoteEndPoint == null)
        {
            throw new InvalidOperationException("Socket is not connected");
        }

        return new AddressInfo
        {
            address = _remoteEndPoint.Address.ToString(),
            family = _remoteEndPoint.AddressFamily == AddressFamily.InterNetworkV6 ? "IPv6" : "IPv4",
            port = _remoteEndPoint.Port
        };
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
