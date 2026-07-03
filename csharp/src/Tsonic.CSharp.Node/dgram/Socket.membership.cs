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

}
