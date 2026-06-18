using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Implements Node.js EventEmitter functionality.
/// All methods follow JavaScript naming conventions (lowercase).
/// </summary>
public partial class EventEmitter
{
    private readonly Dictionary<string, List<Delegate>> _events = new();
    private readonly Dictionary<string, List<Delegate>> _onceWrappers = new();
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
}
