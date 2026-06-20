namespace Tsonic.CSharp.Node;

public partial class EventEmitter
{
    /// <summary>
    /// Removes the specified listener from the listener array for the event named eventName.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="listener">The callback function to remove.</param>
    /// <returns>This EventEmitter instance for chaining.</returns>
    public EventEmitter removeListener(string eventName, Delegate listener)
    {
        if (listener == null)
            throw new ArgumentNullException(nameof(listener));

        if (!_events.ContainsKey(eventName))
            return this;

        _events[eventName].RemoveAll(candidate => ReferenceEquals(candidate.Original, listener));

        // Clean up empty event lists
        if (_events[eventName].Count == 0)
        {
            _events.Remove(eventName);
        }

        // Emit 'removeListener' event
        if (eventName != "removeListener")
        {
            emit("removeListener", eventName, listener);
        }

        return this;
    }
}
