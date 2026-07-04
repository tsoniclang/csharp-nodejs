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

}
