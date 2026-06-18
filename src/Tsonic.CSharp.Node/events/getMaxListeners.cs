namespace Tsonic.CSharp.Node;

public partial class EventEmitter
{
    /// <summary>
    /// Returns the current maximum listener value for this EventEmitter.
    /// </summary>
    /// <returns>The maximum number of listeners.</returns>
    public int getMaxListeners()
    {
        return _maxListeners;
    }
}
