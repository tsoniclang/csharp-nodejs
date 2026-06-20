namespace Tsonic.CSharp.Node;

public partial class EventEmitter
{
    /// <summary>
    /// Adds a listener to the beginning of the listeners array for the specified event.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="listener">The callback function.</param>
    /// <returns>This EventEmitter instance for chaining.</returns>
    public EventEmitter prependListener(string eventName, Delegate listener)
    {
        if (listener == null)
            throw new ArgumentNullException(nameof(listener));

        return addEventListenerCore(eventName, CreateEventListener(listener, once: false), prepend: true);
    }

    /// <inheritdoc cref="prependListener(string, Delegate)" />
    public EventEmitter prependListener(string eventName, Action listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, _ => listener(), once: false), prepend: true);

    /// <inheritdoc cref="prependListener(string, Delegate)" />
    public EventEmitter prependListener(string eventName, Action<object?[]> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(args), once: false), prepend: true);

    /// <inheritdoc cref="prependListener(string, Delegate)" />
    public EventEmitter prependListener<T>(string eventName, Action<T> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(Argument<T>(args, 0)), once: false), prepend: true);

    /// <inheritdoc cref="prependListener(string, Delegate)" />
    public EventEmitter prependListener<T1, T2>(string eventName, Action<T1, T2> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(Argument<T1>(args, 0), Argument<T2>(args, 1)), once: false), prepend: true);

    /// <inheritdoc cref="prependListener(string, Delegate)" />
    public EventEmitter prependListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(Argument<T1>(args, 0), Argument<T2>(args, 1), Argument<T3>(args, 2)), once: false), prepend: true);
}
