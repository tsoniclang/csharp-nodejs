using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Implements Node.js EventEmitter functionality.
/// All methods follow JavaScript naming conventions (lowercase).
/// </summary>
public partial class EventEmitter
{
    private sealed class EventListener
    {
        public EventListener(Delegate original, Action<object?[]> invoke, bool once)
        {
            Original = original;
            Invoke = invoke;
            Once = once;
        }

        public Delegate Original { get; }
        public Action<object?[]> Invoke { get; }
        public bool Once { get; }
    }

    private readonly Dictionary<string, List<EventListener>> _events = new();
    private int _maxListeners = 10;
    private static int _defaultMaxListeners = 10;

    /// <summary>
    /// Gets or sets the default maximum number of listeners for all EventEmitter instances.
    /// </summary>
    public static int defaultMaxListeners
    {
        get => _defaultMaxListeners;
        set => _defaultMaxListeners = value;
    }

    /// <summary>
    /// Creates a Promise that is fulfilled when the EventEmitter emits the given event.
    /// The Promise will resolve with an array of all the arguments emitted to the event.
    /// </summary>
    /// <param name="emitter">The EventEmitter to listen to.</param>
    /// <param name="eventName">The name of the event.</param>
    /// <returns>A Task that resolves with the event arguments.</returns>
    public static Task<object?[]> once(EventEmitter emitter, string eventName)
    {
        if (emitter == null)
            throw new ArgumentNullException(nameof(emitter));
        if (string.IsNullOrEmpty(eventName))
            throw new ArgumentException("Event name cannot be null or empty", nameof(eventName));

        var tcs = new TaskCompletionSource<object?[]>();

        // Create a one-time listener
        Action<object?[]> listener = null!;
        listener = (args) =>
        {
            tcs.TrySetResult(args);
        };

        // Attach the listener using the instance once method
        emitter.once(eventName, listener);

        return tcs.Task;
    }

    private EventEmitter addEventListenerCore(string eventName, EventListener listener, bool prepend)
    {
        if (!_events.ContainsKey(eventName))
        {
            _events[eventName] = new List<EventListener>();
        }

        if (prepend)
        {
            _events[eventName].Insert(0, listener);
        }
        else
        {
            _events[eventName].Add(listener);
        }

        if (eventName != "newListener")
        {
            emit("newListener", eventName, listener.Original);
        }

        if (_events[eventName].Count > _maxListeners && _maxListeners > 0)
        {
            Console.Error.WriteLine(
                $"Warning: Possible EventEmitter memory leak detected. " +
                $"{_events[eventName].Count} {eventName} listeners added. " +
                $"Use emitter.setMaxListeners() to increase limit");
        }

        return this;
    }

    private static EventListener CreateEventListener(Delegate listener, bool once)
    {
        return new EventListener(listener, CreateClosedInvoker(listener), once);
    }

    private static EventListener CreateEventListener(Delegate listener, Action<object?[]> invoke, bool once)
    {
        return new EventListener(listener, invoke, once);
    }

    private static T Argument<T>(object?[] args, int index)
    {
        return index < args.Length ? (T)args[index]! : default!;
    }

    private static Action<object?[]> CreateClosedInvoker(Delegate listener)
    {
        return listener switch
        {
            Action callback => _ => callback(),
            Action<object?> callback => args => callback(args.Length > 0 ? args[0] : null),
            Action<object?, object?> callback => args => callback(
                args.Length > 0 ? args[0] : null,
                args.Length > 1 ? args[1] : null),
            Action<object?[]> callback => args => callback(args),
            _ => throw new NotSupportedException("EventEmitter supports only closed Action callback shapes.")
        };
    }

    private static Delegate[] ListenerDelegates(IReadOnlyCollection<EventListener> listeners)
    {
        return listeners.Select(listener => listener.Original).ToArray();
    }

    private void removeStoredListener(string eventName, EventListener listener)
    {
        if (!_events.TryGetValue(eventName, out var listeners))
            return;

        listeners.Remove(listener);
        if (listeners.Count == 0)
        {
            _events.Remove(eventName);
        }
    }
}
