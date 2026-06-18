namespace Tsonic.CSharp.Node;

/// <summary>
/// Information about the remote endpoint that sent a datagram.
/// </summary>
public class RemoteInfo
{
    /// <summary>
    /// The IP address of the remote endpoint.
    /// </summary>
    public string address { get; set; } = "";

    /// <summary>
    /// The address family ('IPv4' or 'IPv6').
    /// </summary>
    public string family { get; set; } = "IPv4";

    /// <summary>
    /// The port number of the remote endpoint.
    /// </summary>
    public int port { get; set; }

    /// <summary>
    /// The size of the message in bytes.
    /// </summary>
    public int size { get; set; }
}
