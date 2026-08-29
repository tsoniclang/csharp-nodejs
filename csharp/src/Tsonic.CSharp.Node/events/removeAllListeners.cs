namespace Tsonic.CSharp.Node;

using Tsonic.CSharp.Runtime;

public partial class EventEmitter
{
    /// <summary>Removes every listener registered for the selected event.</summary>
    public EventEmitter removeAllListeners(TsValue eventName) =>
        removeAllListenersCore(EventKey(eventName));

    /// <summary>
    /// Removes all listeners, or those of the specified eventName.
    /// </summary>
    /// <param name="eventName">Optional event name. If not provided, removes all listeners for all events.</param>
    /// <returns>This EventEmitter instance for chaining.</returns>
    public EventEmitter removeAllListeners(string? eventName = null)
    {
        if (eventName == null)
        {
            object[] eventNames;
            lock (_eventLock)
                eventNames = _events.Keys.ToArray();
            foreach (var name in eventNames)
                removeAllListenersCore(name);
        }
        else
        {
            removeAllListenersCore(EventKey(eventName));
        }

        return this;
    }

    private EventEmitter removeAllListenersCore(object eventName)
    {
        EventListener[] listeners;
        lock (_eventLock)
        {
            if (!_events.TryGetValue(eventName, out var registered))
                return this;
            listeners = registered.ToArray();
            _events.Remove(eventName);
        }
        if (!IsEvent(eventName, "removeListener"))
            foreach (var listener in listeners.Reverse())
                emit("removeListener", EventValue(eventName), listener.Original);
        return this;
    }
}
