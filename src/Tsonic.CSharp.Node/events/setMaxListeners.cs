namespace Tsonic.CSharp.Node;

public partial class EventEmitter
{
    /// <summary>
    /// Sets the maximum number of listeners that can be added for a single event.
    /// </summary>
    /// <param name="n">The maximum number of listeners. Set to 0 for unlimited.</param>
    /// <returns>This EventEmitter instance for chaining.</returns>
    public EventEmitter setMaxListeners(int n)
    {
        if (n < 0)
            throw new ArgumentException("Max listeners must be non-negative", nameof(n));

        _maxListeners = n;
        return this;
    }
}
