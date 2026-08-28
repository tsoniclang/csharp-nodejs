using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tsonic.CSharp.Js;
using Tsonic.CSharp.Runtime;

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

    private readonly Dictionary<object, List<EventListener>> _events = new();
    private readonly object _eventLock = new();
    private int _maxListeners = _defaultMaxListeners;
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
        ArgumentNullException.ThrowIfNull(eventName);

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

    public static Task<object?[]> once(EventEmitter emitter, TsValue eventName)
    {
        ArgumentNullException.ThrowIfNull(emitter);
        var completion = new TaskCompletionSource<object?[]>();
        Action<object?[]> listener = arguments => completion.TrySetResult(arguments);
        emitter.once(eventName, listener);
        return completion.Task;
    }

    private EventEmitter addEventListenerCore(object eventName, EventListener listener, bool prepend)
    {
        if (!IsEvent(eventName, "newListener"))
        {
            emit("newListener", EventValue(eventName), listener.Original);
        }

        int count;
        lock (_eventLock)
        {
            if (!_events.TryGetValue(eventName, out var listeners))
            {
                listeners = new List<EventListener>();
                _events[eventName] = listeners;
            }

            if (prepend)
                listeners.Insert(0, listener);
            else
                listeners.Add(listener);
            count = listeners.Count;
        }

        if (count > _maxListeners && _maxListeners > 0)
        {
            Console.Error.WriteLine(
                $"Warning: Possible EventEmitter memory leak detected. " +
                $"{count} {eventName} listeners added. " +
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

    private void removeStoredListener(object eventName, EventListener listener)
    {
        lock (_eventLock)
        {
            if (!_events.TryGetValue(eventName, out var listeners))
                return;

            listeners.Remove(listener);
            if (listeners.Count == 0)
                _events.Remove(eventName);
        }
    }

    private static object EventKey(string eventName)
    {
        ArgumentNullException.ThrowIfNull(eventName);
        return eventName;
    }

    private static object EventKey(TsValue eventName)
    {
        if (TsValue.IsDynamicInstanceOf<string>(eventName))
            return TsValue.CastDynamic<string>(eventName);
        if (TsValue.IsDynamicInstanceOf<Symbol>(eventName))
            return TsValue.CastDynamic<Symbol>(eventName);
        throw new TypeError("An EventEmitter event name must be a string or Symbol.");
    }

    private static TsValue EventValue(object eventName) => TsValue.from(eventName);

    private static bool IsEvent(object eventName, string name) =>
        eventName is string text && string.Equals(text, name, StringComparison.Ordinal);
}
