namespace Tsonic.CSharp.Node;

public partial class EventEmitter
{
    /// <summary>
    /// Returns an array listing the events for which the emitter has registered listeners.
    /// </summary>
    /// <returns>Array of event names.</returns>
    public string[] eventNames()
    {
        return _events.Keys.ToArray();
    }
}
