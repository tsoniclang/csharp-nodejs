namespace Tsonic.CSharp.Node;

public partial class EventEmitter
{
    /// <summary>
    /// Adds a listener to the beginning of the listeners array for the specified event.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="listener">The callback function.</param>
    /// <returns>This EventEmitter instance for chaining.</returns>
    public EventEmitter prependListener(string eventName, Delegate listener)
    {
        if (listener == null)
            throw new ArgumentNullException(nameof(listener));

        if (!_events.ContainsKey(eventName))
        {
            _events[eventName] = new List<Delegate>();
        }

        _events[eventName].Insert(0, listener);

        // Emit 'newListener' event
        if (eventName != "newListener")
        {
            emit("newListener", eventName, listener);
        }

        return this;
    }
}
