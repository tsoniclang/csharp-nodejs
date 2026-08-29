namespace Tsonic.CSharp.Node;

public partial class TLSSocket
{
    /// <summary>Destroys the TLS socket and optionally emits the selected error.</summary>
    public new TLSSocket destroy(Exception? error = null)
    {
        if (destroyed)
            return this;

        _sslStream?.Dispose();
        _baseSocket?.destroy();
        base.destroy(error);
        return this;
    }
}
