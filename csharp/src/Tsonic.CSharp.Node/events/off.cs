namespace Tsonic.CSharp.Node;

using Tsonic.CSharp.Runtime;

public partial class EventEmitter
{
    public EventEmitter off(TsValue eventName, Delegate listener) => removeListener(eventName, listener);

    /// <summary>
    /// Alias for removeListener().
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="listener">The callback function to remove.</param>
    /// <returns>This EventEmitter instance for chaining.</returns>
    public EventEmitter off(string eventName, Delegate listener) => removeListener(eventName, listener);
}
