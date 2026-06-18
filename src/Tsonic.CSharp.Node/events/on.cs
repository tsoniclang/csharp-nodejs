namespace Tsonic.CSharp.Node;

public partial class EventEmitter
{
    /// <summary>
    /// Adds a listener function to the end of the listeners array for the specified event.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="listener">The callback function.</param>
    /// <returns>This EventEmitter instance for chaining.</returns>
    public EventEmitter on(string eventName, Delegate listener)
    {
        if (listener == null)
            throw new ArgumentNullException(nameof(listener));

        if (!_events.ContainsKey(eventName))
        {
            _events[eventName] = new List<Delegate>();
        }

        _events[eventName].Add(listener);

        // Emit 'newListener' event
        if (eventName != "newListener")
        {
            emit("newListener", eventName, listener);
        }

        // Warn if max listeners exceeded
        if (_events[eventName].Count > _maxListeners && _maxListeners > 0)
        {
            Console.Error.WriteLine(
                $"Warning: Possible EventEmitter memory leak detected. " +
                $"{_events[eventName].Count} {eventName} listeners added. " +
                $"Use emitter.setMaxListeners() to increase limit");
        }

        return this;
    }
}
