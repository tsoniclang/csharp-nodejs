namespace Tsonic.CSharp.Node;

public partial class EventEmitter
{
    /// <summary>
    /// Returns a copy of the array of listeners for the event named eventName.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <returns>Array of listener functions.</returns>
    public Delegate[] listeners(string eventName)
    {
        if (!_events.ContainsKey(eventName))
            return Array.Empty<Delegate>();

        return ListenerDelegates(_events[eventName]);
    }
}
