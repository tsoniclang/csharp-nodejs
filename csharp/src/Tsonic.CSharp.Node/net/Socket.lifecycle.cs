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

}
