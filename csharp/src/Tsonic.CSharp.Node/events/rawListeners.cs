namespace Tsonic.CSharp.Node;

using Tsonic.CSharp.Runtime;

public partial class EventEmitter
{
    /// <summary>Returns the raw listeners registered for the selected event.</summary>
    public Delegate[] rawListeners(TsValue eventName) => listeners(eventName);

    /// <summary>
    /// Returns a copy of the array of listeners for the event, including any wrappers.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <returns>Array of listener functions.</returns>
    public Delegate[] rawListeners(string eventName)
    {
        // For now, same as listeners() since we handle wrappers internally
        return listeners(eventName);
    }
}
