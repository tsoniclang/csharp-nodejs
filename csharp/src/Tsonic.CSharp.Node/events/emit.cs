namespace Tsonic.CSharp.Node;

using System.Linq;
using Tsonic.CSharp.Runtime;

public partial class EventEmitter
{
    /// <summary>Synchronously dispatches an event to its registered listeners.</summary>
    public bool emit(TsValue eventName, params TsValue[] args) =>
        emitCore(EventKey(eventName), args.Cast<object?>().ToArray());

    /// <summary>
    /// Synchronously calls each of the listeners registered for the event named eventName,
    /// in the order they were registered, passing the supplied arguments to each.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="args">Arguments to pass to the listeners.</param>
    /// <returns>True if the event had listeners, false otherwise.</returns>
    public bool emit(string eventName, params object?[] args)
    {
        return emitCore(EventKey(eventName), args);
    }

    private bool emitCore(object eventName, object?[] args)
    {
        EventListener[]? listeners;
        lock (_eventLock)
        {
            if (!_events.TryGetValue(eventName, out var registered))
            {
                listeners = null;
            }
            else
            {
                listeners = registered.Entries;
            }
        }

        if (listeners == null)
        {
            if (IsEvent(eventName, "error"))
            {
                var error = args.Length > 0 ? args[0] : null;
                if (error is Exception exception)
                    throw exception;
                throw new Exception($"Uncaught, unspecified 'error' event. ({error})");
            }
            return false;
        }

        var dispatched = false;
        var consumed = false;
        try
        {
            foreach (var listener in listeners)
            {
                if (listener.Once)
                {
                    if (!ConsumeOnceListener(eventName, listener))
                        continue;
                    consumed = true;
                }
                listener.Invoke(args);
                dispatched = true;
            }
        }
        finally
        {
            if (consumed)
                CompactListeners(eventName);
        }

        return dispatched;
    }

    private bool ConsumeOnceListener(object eventName, EventListener listener)
    {
        lock (_eventLock)
        {
            if (listener.Consumed)
                return false;
            listener.Consumed = true;
            if (listener.Registered && _events.TryGetValue(eventName, out var registered))
            {
                listener.Registered = false;
                if (registered.Count == 1)
                    _events.Remove(eventName);
                else
                    _events[eventName] = registered with { Count = registered.Count - 1 };
            }
            return true;
        }
    }
}
