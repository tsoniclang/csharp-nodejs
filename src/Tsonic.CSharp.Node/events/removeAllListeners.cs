namespace Tsonic.CSharp.Node;

public partial class EventEmitter
{
    /// <summary>
    /// Removes all listeners, or those of the specified eventName.
    /// </summary>
    /// <param name="eventName">Optional event name. If not provided, removes all listeners for all events.</param>
    /// <returns>This EventEmitter instance for chaining.</returns>
    public EventEmitter removeAllListeners(string? eventName = null)
    {
        if (eventName == null)
        {
            // Remove all listeners for all events
            var eventNames = _events.Keys.ToList();
            foreach (var name in eventNames)
            {
                removeAllListeners(name);
            }
            _events.Clear();
            _onceWrappers.Clear();
        }
        else
        {
            // Remove all listeners for specific event
            if (_events.ContainsKey(eventName))
            {
                var listeners = _events[eventName].ToList();
                foreach (var listener in listeners)
                {
                    emit("removeListener", eventName, listener);
                }
                _events.Remove(eventName);
            }

            if (_onceWrappers.ContainsKey(eventName))
            {
                _onceWrappers.Remove(eventName);
            }
        }

        return this;
    }
}
