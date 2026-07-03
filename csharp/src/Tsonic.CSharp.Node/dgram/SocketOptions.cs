namespace Tsonic.CSharp.Node;

/// <summary>
/// Options for creating a dgram socket.
/// </summary>
public class SocketOptions
{
    /// <summary>
    /// The type of socket - 'udp4' or 'udp6'.
    /// </summary>
    public string type { get; set; } = "udp4";

    /// <summary>
    /// If true, the socket will reuse the address.
    /// </summary>
    public bool reuseAddr { get; set; } = false;

    /// <summary>
    /// If true, the socket will reuse the port (SO_REUSEPORT).
    /// </summary>
    public bool reusePort { get; set; } = false;

    /// <summary>
    /// For IPv6 sockets, if true the socket will only receive IPv6 traffic.
    /// </summary>
    public bool ipv6Only { get; set; } = false;

    /// <summary>
    /// Sets the SO_RCVBUF socket receive buffer size in bytes.
    /// </summary>
    public int? recvBufferSize { get; set; }

    /// <summary>
    /// Sets the SO_SNDBUF socket send buffer size in bytes.
    /// </summary>
    public int? sendBufferSize { get; set; }
}

/// <summary>
/// Options for binding a dgram socket.
/// </summary>
public class BindOptions
{
    /// <summary>
    /// The port to bind to.
    /// </summary>
    public int? port { get; set; }

    /// <summary>
    /// The address to bind to.
    /// </summary>
    public string? address { get; set; }

    /// <summary>
    /// If true, the socket will be bound exclusively.
    /// </summary>
    public bool exclusive { get; set; } = false;

    /// <summary>
    /// File descriptor (not supported in .NET).
    /// </summary>
    public int? fd { get; set; }
}
