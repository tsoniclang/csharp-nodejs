namespace Tsonic.CSharp.Node;

using Tsonic.CSharp.Runtime;

public partial class EventEmitter
{
    /// <summary>Registers a persistent listener for the selected event.</summary>
    public EventEmitter addListener(TsValue eventName, Delegate listener) => on(eventName, listener);
    /// <inheritdoc cref="addListener(TsValue, Delegate)" />
    public EventEmitter addListener(TsValue eventName, Action listener) => on(eventName, listener);
    /// <inheritdoc cref="addListener(TsValue, Delegate)" />
    public EventEmitter addListener(TsValue eventName, Action<TsValue[]> listener) => on(eventName, listener);
    /// <inheritdoc cref="addListener(TsValue, Delegate)" />
    public EventEmitter addListener<T>(TsValue eventName, Action<T> listener) => on(eventName, listener);
    /// <inheritdoc cref="addListener(TsValue, Delegate)" />
    public EventEmitter addListener<T1, T2>(TsValue eventName, Action<T1, T2> listener) => on(eventName, listener);
    /// <inheritdoc cref="addListener(TsValue, Delegate)" />
    public EventEmitter addListener<T1, T2, T3>(TsValue eventName, Action<T1, T2, T3> listener) => on(eventName, listener);

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
