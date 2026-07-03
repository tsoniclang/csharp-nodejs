using System;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// Address information returned by server.address() and socket.address().
/// </summary>
public class AddressInfo
{
    /// <summary>
    /// The IP address.
    /// </summary>
    public string address { get; set; } = string.Empty;

    /// <summary>
    /// The address family ("IPv4" or "IPv6").
    /// </summary>
    public string family { get; set; } = string.Empty;

    /// <summary>
    /// The port number.
    /// </summary>
    public int port { get; set; }
}

/// <summary>
/// Options for Socket constructor.
/// </summary>
public class SocketConstructorOpts
{
    /// <summary>
    /// File descriptor (not supported in .NET).
    /// </summary>
    public int? fd { get; set; }

    /// <summary>
    /// Allow half-open TCP connections.
    /// </summary>
    public bool? allowHalfOpen { get; set; }

    /// <summary>
    /// Whether the socket is readable.
    /// </summary>
    public bool? readable { get; set; }

    /// <summary>
    /// Whether the socket is writable.
    /// </summary>
    public bool? writable { get; set; }
}

/// <summary>
/// TCP socket connection options.
/// </summary>
public class TcpSocketConnectOpts
{
    /// <summary>
    /// Port to connect to (required).
    /// </summary>
    public int port { get; set; }

    /// <summary>
    /// Host to connect to.
    /// </summary>
    public string? host { get; set; }

    /// <summary>
    /// Local address to bind to.
    /// </summary>
    public string? localAddress { get; set; }

    /// <summary>
    /// Local port to bind to.
    /// </summary>
    public int? localPort { get; set; }

    /// <summary>
    /// DNS lookup hints.
    /// </summary>
    public int? hints { get; set; }

    /// <summary>
    /// IP family (4 or 6).
    /// </summary>
    public int? family { get; set; }

    /// <summary>
    /// Disable Nagle's algorithm.
    /// </summary>
    public bool? noDelay { get; set; }

    /// <summary>
    /// Enable keep-alive.
    /// </summary>
    public bool? keepAlive { get; set; }

    /// <summary>
    /// Keep-alive initial delay in milliseconds.
    /// </summary>
    public int? keepAliveInitialDelay { get; set; }
}

/// <summary>
/// IPC socket connection options.
/// </summary>
public class IpcSocketConnectOpts
{
    /// <summary>
    /// Path to IPC socket.
    /// </summary>
    public string path { get; set; } = string.Empty;
}

/// <summary>
/// Options for server.listen().
/// </summary>
public class ListenOptions
{
    /// <summary>
    /// Port to listen on.
    /// </summary>
    public int? port { get; set; }

    /// <summary>
    /// Host to listen on.
    /// </summary>
    public string? host { get; set; }

    /// <summary>
    /// Path for IPC server.
    /// </summary>
    public string? path { get; set; }

    /// <summary>
    /// Backlog queue size.
    /// </summary>
    public int? backlog { get; set; }

    /// <summary>
    /// Whether IPv6 connections should be enabled.
    /// </summary>
    public bool? ipv6Only { get; set; }
}

/// <summary>
/// Options for Server constructor.
/// </summary>
public class ServerOpts
{
    /// <summary>
    /// Allow half-open TCP connections.
    /// </summary>
    public bool? allowHalfOpen { get; set; }

    /// <summary>
    /// Pause incoming connections.
    /// </summary>
    public bool? pauseOnConnect { get; set; }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
