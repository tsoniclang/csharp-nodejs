#pragma warning disable CS1591
#pragma warning disable IDE1006

namespace Tsonic.CSharp.Node;

public sealed class CipherInfo
{
    public string name { get; set; } = string.Empty;

    public string standardName { get; set; } = string.Empty;

    public string version { get; set; } = string.Empty;

    public int? nid { get; set; }

    public int? blockSize { get; set; }

    public int? ivLength { get; set; }

    public int? keyLength { get; set; }

    public string? mode { get; set; }
}

public static partial class tls
{
    public static readonly int defaultPort = 443;

    public static TLSSocket connectGet(ConnectionOptions options)
    {
        return connect(options);
    }
}
