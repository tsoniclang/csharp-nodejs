#pragma warning disable CS1591
#pragma warning disable IDE1006

namespace Tsonic.CSharp.Node;

public sealed class ConnectOptions : TcpSocketConnectOpts
{
    public ConnectOptions()
    {
    }

    public ConnectOptions(int port, string? host = null)
    {
        this.port = port;
        this.host = host;
    }
}

public sealed class LookupEndpoint
{
    public string address { get; set; } = string.Empty;

    public int family { get; set; }
}
