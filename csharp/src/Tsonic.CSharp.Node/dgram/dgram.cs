using System;
using System.Net;
using System.Net.Sockets;

namespace Tsonic.CSharp.Node;

/// <summary>
/// The dgram module provides an implementation of UDP datagram sockets.
/// </summary>
public static class dgram
{
    /// <summary>
    /// Creates a dgram.Socket object. The type argument can be either 'udp4' or 'udp6'.
    /// </summary>
    /// <param name="type">Socket type - 'udp4' or 'udp6'</param>
    /// <param name="callback">Optional callback attached as a listener for 'message' events</param>
    /// <returns>A new DgramSocket instance</returns>
    public static DgramSocket createSocket(string type, Action<byte[], RemoteInfo>? callback = null)
    {
        return new DgramSocket(type, callback);
    }

    /// <summary>
    /// Creates a dgram.Socket object with options.
    /// </summary>
    /// <param name="options">Socket options</param>
    /// <param name="callback">Optional callback attached as a listener for 'message' events</param>
    /// <returns>A new DgramSocket instance</returns>
    public static DgramSocket createSocket(SocketOptions options, Action<byte[], RemoteInfo>? callback = null)
    {
        return new DgramSocket(options, callback);
    }
}
