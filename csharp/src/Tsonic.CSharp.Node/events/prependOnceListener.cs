namespace Tsonic.CSharp.Node;

using System.Linq;
using Tsonic.CSharp.Runtime;

public partial class EventEmitter
{
    public EventEmitter prependOnceListener(TsValue eventName, Delegate listener) =>
        addEventListenerCore(EventKey(eventName), CreateEventListener(listener, once: true), prepend: true);

    public EventEmitter prependOnceListener(TsValue eventName, Action listener) =>
        addEventListenerCore(EventKey(eventName), CreateEventListener(listener, _ => listener(), once: true), prepend: true);

    public EventEmitter prependOnceListener(TsValue eventName, Action<TsValue[]> listener) =>
        addEventListenerCore(EventKey(eventName), CreateEventListener(listener, args => listener(args.Select(TsValue.from).ToArray()), once: true), prepend: true);

    public EventEmitter prependOnceListener<T>(TsValue eventName, Action<T> listener) =>
        addEventListenerCore(EventKey(eventName), CreateEventListener(listener, args => listener(Argument<T>(args, 0)), once: true), prepend: true);

    public EventEmitter prependOnceListener<T1, T2>(TsValue eventName, Action<T1, T2> listener) =>
        addEventListenerCore(EventKey(eventName), CreateEventListener(listener, args => listener(Argument<T1>(args, 0), Argument<T2>(args, 1)), once: true), prepend: true);

    public EventEmitter prependOnceListener<T1, T2, T3>(TsValue eventName, Action<T1, T2, T3> listener) =>
        addEventListenerCore(EventKey(eventName), CreateEventListener(listener, args => listener(Argument<T1>(args, 0), Argument<T2>(args, 1), Argument<T3>(args, 2)), once: true), prepend: true);

    /// <summary>
    /// Adds a one-time listener to the beginning of the listeners array for the specified event.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="listener">The callback function.</param>
    /// <returns>This EventEmitter instance for chaining.</returns>
    public EventEmitter prependOnceListener(string eventName, Delegate listener)
    {
        if (listener == null)
            throw new ArgumentNullException(nameof(listener));

        return addEventListenerCore(eventName, CreateEventListener(listener, once: true), prepend: true);
    }

    /// <inheritdoc cref="prependOnceListener(string, Delegate)" />
    public EventEmitter prependOnceListener(string eventName, Action listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, _ => listener(), once: true), prepend: true);

    /// <inheritdoc cref="prependOnceListener(string, Delegate)" />
    public EventEmitter prependOnceListener(string eventName, Action<object?[]> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(args), once: true), prepend: true);

    /// <inheritdoc cref="prependOnceListener(string, Delegate)" />
    public EventEmitter prependOnceListener<T>(string eventName, Action<T> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(Argument<T>(args, 0)), once: true), prepend: true);

    /// <inheritdoc cref="prependOnceListener(string, Delegate)" />
    public EventEmitter prependOnceListener<T1, T2>(string eventName, Action<T1, T2> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(Argument<T1>(args, 0), Argument<T2>(args, 1)), once: true), prepend: true);

    /// <inheritdoc cref="prependOnceListener(string, Delegate)" />
    public EventEmitter prependOnceListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(Argument<T1>(args, 0), Argument<T2>(args, 1), Argument<T3>(args, 2)), once: true), prepend: true);
}
