namespace Tsonic.CSharp.Node;

public partial class EventEmitter
{
    /// <summary>
    /// Adds a listener function to the end of the listeners array for the specified event.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="listener">The callback function.</param>
    /// <returns>This EventEmitter instance for chaining.</returns>
    public EventEmitter on(string eventName, Delegate listener)
    {
        if (listener == null)
            throw new ArgumentNullException(nameof(listener));

        return addEventListenerCore(eventName, CreateEventListener(listener, once: false), prepend: false);
    }

    /// <inheritdoc cref="on(string, Delegate)" />
    public EventEmitter on(string eventName, Action listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, _ => listener(), once: false), prepend: false);

    /// <inheritdoc cref="on(string, Delegate)" />
    public EventEmitter on(string eventName, Action<object?[]> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(args), once: false), prepend: false);

    /// <inheritdoc cref="on(string, Delegate)" />
    public EventEmitter on<T>(string eventName, Action<T> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(Argument<T>(args, 0)), once: false), prepend: false);

    /// <inheritdoc cref="on(string, Delegate)" />
    public EventEmitter on<T1, T2>(string eventName, Action<T1, T2> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(Argument<T1>(args, 0), Argument<T2>(args, 1)), once: false), prepend: false);

    /// <inheritdoc cref="on(string, Delegate)" />
    public EventEmitter on<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener) =>
        addEventListenerCore(eventName, CreateEventListener(listener, args => listener(Argument<T1>(args, 0), Argument<T2>(args, 1), Argument<T3>(args, 2)), once: false), prepend: false);
}
