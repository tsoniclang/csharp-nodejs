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

        // Create a reference to the wrapper so we can remove it
        Action<object?[]>? wrapper = null;
        wrapper = (args) =>
        {
            // Remove the wrapper itself
            if (wrapper != null)
            {
                removeListener(eventName, wrapper);
            }

            // Invoke the original listener
            var method = listener.Method;
            var parameters = method.GetParameters();

            if (parameters.Length == 0)
            {
                listener.DynamicInvoke();
            }
            else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(object?[]))
            {
                listener.DynamicInvoke(new object?[] { args });
            }
            else
            {
                listener.DynamicInvoke(args);
            }
        };

        // Store the mapping so we can remove it if needed
        if (!_onceWrappers.ContainsKey(eventName))
        {
            _onceWrappers[eventName] = new List<Delegate>();
        }
        _onceWrappers[eventName].Add(wrapper);

        on(eventName, wrapper);
        return this;
    }
}
