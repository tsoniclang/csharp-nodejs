namespace Tsonic.CSharp.Node;

public partial class TLSSocket
{
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
