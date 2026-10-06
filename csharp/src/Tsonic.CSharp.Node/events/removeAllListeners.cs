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
            listeners = registered.Entries;
            foreach (var listener in registered.Entries)
                listener.Registered = false;
            _events.Remove(eventName);
        }
        if (!IsEvent(eventName, "removeListener"))
            for (var index = listeners.Length - 1; index >= 0; index--)
                if (!listeners[index].Consumed)
                    emit("removeListener", EventValue(eventName), listeners[index].Original);
        return this;
    }
}
