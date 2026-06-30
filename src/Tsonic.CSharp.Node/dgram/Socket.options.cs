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

}
