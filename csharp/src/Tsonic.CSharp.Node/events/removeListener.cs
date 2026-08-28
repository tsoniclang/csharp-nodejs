namespace Tsonic.CSharp.Node;

using Tsonic.CSharp.Runtime;

public partial class EventEmitter
{
    public EventEmitter removeListener(TsValue eventName, Delegate listener) =>
        removeListenerCore(EventKey(eventName), listener);

    /// <summary>
    /// Removes the specified listener from the listener array for the event named eventName.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="listener">The callback function to remove.</param>
    /// <returns>This EventEmitter instance for chaining.</returns>
    public EventEmitter removeListener(string eventName, Delegate listener)
    {
        return removeListenerCore(EventKey(eventName), listener);
    }

    private EventEmitter removeListenerCore(object eventName, Delegate listener)
    {
        if (listener == null)
            throw new ArgumentNullException(nameof(listener));

        var removed = false;
        lock (_eventLock)
        {
            if (!_events.TryGetValue(eventName, out var listeners))
                return this;

            for (var index = listeners.Count - 1; index >= 0; index--)
            {
                if (!Equals(listeners[index].Original, listener))
                    continue;
                listeners.RemoveAt(index);
                removed = true;
                break;
            }
            if (listeners.Count == 0)
                _events.Remove(eventName);
        }

        if (removed && !IsEvent(eventName, "removeListener"))
            emit("removeListener", EventValue(eventName), listener);

        return this;
    }
}
