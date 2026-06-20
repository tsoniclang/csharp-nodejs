namespace Tsonic.CSharp.Node;

public partial class EventEmitter
{
    /// <summary>
    /// Adds a listener function that will be invoked only once for the specified event.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="listener">The callback function.</param>
    /// <returns>This EventEmitter instance for chaining.</returns>
    public EventEmitter once(string eventName, Delegate listener)
    {
        if (listener == null)
            throw new ArgumentNullException(nameof(listener));

        return addEventListenerCore(eventName, CreateEventListener(listener, once: true), prepend: false);
    }

    /// <inheritdoc cref="once(string, Delegate)" />
    public EventEmitter once(string eventName, Action listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, _ => listener(), once: true), prepend: false);

    /// <inheritdoc cref="once(string, Delegate)" />
    public EventEmitter once(string eventName, Action<object?[]> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(args), once: true), prepend: false);

    /// <inheritdoc cref="once(string, Delegate)" />
    public EventEmitter once<T>(string eventName, Action<T> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(Argument<T>(args, 0)), once: true), prepend: false);

    /// <inheritdoc cref="once(string, Delegate)" />
    public EventEmitter once<T1, T2>(string eventName, Action<T1, T2> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(Argument<T1>(args, 0), Argument<T2>(args, 1)), once: true), prepend: false);

    /// <inheritdoc cref="once(string, Delegate)" />
    public EventEmitter once<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(Argument<T1>(args, 0), Argument<T2>(args, 1), Argument<T3>(args, 2)), once: true), prepend: false);
}
