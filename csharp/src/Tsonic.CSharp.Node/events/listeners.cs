namespace Tsonic.CSharp.Node;

using Tsonic.CSharp.Runtime;

public partial class EventEmitter
{
    public Delegate[] listeners(TsValue eventName) => listenersCore(EventKey(eventName));

    /// <summary>
    /// Returns a copy of the array of listeners for the event named eventName.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <returns>Array of listener functions.</returns>
    public Delegate[] listeners(string eventName)
    {
        return listenersCore(EventKey(eventName));
    }

    private Delegate[] listenersCore(object eventName)
    {
        lock (_eventLock)
            return _events.TryGetValue(eventName, out var listeners)
                ? ListenerDelegates(listeners)
                : Array.Empty<Delegate>();
    }
}
