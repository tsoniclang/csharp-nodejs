namespace Tsonic.CSharp.Node;

public partial class TLSSocket
{
    private int _tlsClosed;
    /// <summary>Destroys the TLS socket and optionally emits the selected error.</summary>
    public new TLSSocket destroy(Exception? error = null)
    {
        if (System.Threading.Interlocked.Exchange(ref _tlsClosed, 1) != 0)
            return this;

        _sslStream?.Dispose();
        _baseSocket?.destroy();
        base.destroy(error);
        _tlsRegistration.Dispose();
        return this;
    }
}
