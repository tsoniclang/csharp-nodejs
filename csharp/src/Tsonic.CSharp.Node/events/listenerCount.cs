namespace Tsonic.CSharp.Node;

using Tsonic.CSharp.Runtime;

public partial class EventEmitter
{
    public int listenerCount(TsValue eventName) => listenerCountCore(EventKey(eventName));

    /// <summary>
    /// Returns the number of listeners listening to the event named eventName.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <returns>The number of listeners.</returns>
    public int listenerCount(string eventName)
    {
        return listenerCountCore(EventKey(eventName));
    }

    private int listenerCountCore(object eventName)
    {
        lock (_eventLock)
            return _events.TryGetValue(eventName, out var listeners) ? listeners.Count : 0;
    }
}
