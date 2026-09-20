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
            if (!_events.TryGetValue(eventName, out var registered) || registered.Length == 0)
            {
                listeners = null;
            }
            else
            {
                listeners = registered;
                if (System.Array.Exists(registered, static listener => listener.Once))
                {
                    var retained = System.Array.FindAll(registered, static listener => !listener.Once);
                    if (retained.Length == 0)
                        _events.Remove(eventName);
                    else
                        _events[eventName] = retained;
                }
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

        foreach (var listener in listeners)
            listener.Invoke(args);

        return true;
    }
}
