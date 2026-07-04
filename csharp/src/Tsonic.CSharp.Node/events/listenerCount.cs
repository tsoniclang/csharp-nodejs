namespace Tsonic.CSharp.Node;

public partial class EventEmitter
{
    /// <summary>
    /// Returns the number of listeners listening to the event named eventName.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <returns>The number of listeners.</returns>
    public int listenerCount(string eventName)
    {
        if (!_events.ContainsKey(eventName))
            return 0;

        return _events[eventName].Count;
    }
}
