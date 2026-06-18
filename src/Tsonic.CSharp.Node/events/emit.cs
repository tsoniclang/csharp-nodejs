namespace Tsonic.CSharp.Node;

public partial class EventEmitter
{
    /// <summary>
    /// Synchronously calls each of the listeners registered for the event named eventName,
    /// in the order they were registered, passing the supplied arguments to each.
    /// </summary>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="args">Arguments to pass to the listeners.</param>
    /// <returns>True if the event had listeners, false otherwise.</returns>
    public bool emit(string eventName, params object?[] args)
    {
        if (!_events.ContainsKey(eventName) || _events[eventName].Count == 0)
        {
            // Special handling for 'error' event
            if (eventName == "error")
            {
                var error = args.Length > 0 ? args[0] : null;
                if (error is Exception ex)
                {
                    throw ex;
                }
                throw new Exception($"Uncaught, unspecified 'error' event. ({error})");
            }
            return false;
        }

        // Create a copy to avoid modification during iteration
        var listeners = _events[eventName].ToList();

        foreach (var listener in listeners)
        {
            try
            {
                // Check if listener expects parameters
                var method = listener.Method;
                var parameters = method.GetParameters();

                if (parameters.Length == 0)
                {
                    // No parameters expected
                    listener.DynamicInvoke();
                }
                else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(object?[]))
                {
                    // Single params array parameter
                    listener.DynamicInvoke(new object?[] { args });
                }
                else
                {
                    // Regular parameters - pass args directly
                    listener.DynamicInvoke(args);
                }
            }
            catch (Exception ex)
            {
                // If an error listener throws, emit it as an error event
                if (eventName != "error")
                {
                    emit("error", ex);
                }
                else
                {
                    throw;
                }
            }
        }

        return true;
    }
}
