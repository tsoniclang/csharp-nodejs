namespace Tsonic.CSharp.Node;

public partial class EventEmitter
{
    /// <summary>
    /// Alias for on(). Adds a listener to the end of the listeners array.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="listener">The callback function.</param>
    /// <returns>This EventEmitter instance for chaining.</returns>
    public EventEmitter addListener(string eventName, Delegate listener) => on(eventName, listener);
}
