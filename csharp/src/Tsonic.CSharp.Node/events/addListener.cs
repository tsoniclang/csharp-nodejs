namespace Tsonic.CSharp.Node;

public partial class EventEmitter
{
    /// <summary>
    /// Alias for on(). Adds a listener to the end of the listeners array.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="listener">The callback function.</param>
    /// <returns>This EventEmitter instance for chaining.</returns>
    public EventEmitter addListener(string eventName, Delegate listener) => on(eventName, listener);
    /// <inheritdoc cref="addListener(string, Delegate)" />
    public EventEmitter addListener(string eventName, Action listener) => on(eventName, listener);
    /// <inheritdoc cref="addListener(string, Delegate)" />
    public EventEmitter addListener(string eventName, Action<object?[]> listener) => on(eventName, listener);
    /// <inheritdoc cref="addListener(string, Delegate)" />
    public EventEmitter addListener<T>(string eventName, Action<T> listener) => on(eventName, listener);
    /// <inheritdoc cref="addListener(string, Delegate)" />
    public EventEmitter addListener<T1, T2>(string eventName, Action<T1, T2> listener) => on(eventName, listener);
    /// <inheritdoc cref="addListener(string, Delegate)" />
    public EventEmitter addListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> listener) => on(eventName, listener);
}
