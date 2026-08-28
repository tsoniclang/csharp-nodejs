namespace Tsonic.CSharp.Node;

using Tsonic.CSharp.Js;
using Tsonic.CSharp.Runtime;

public partial class EventEmitter
{
    /// <summary>
    /// Returns an array listing the events for which the emitter has registered listeners.
    /// </summary>
    /// <returns>Array of event names.</returns>
    public JSArray<TsValue> eventNames()
    {
        lock (_eventLock)
            return new JSArray<TsValue>(_events.Keys.Select(EventValue));
    }
}
