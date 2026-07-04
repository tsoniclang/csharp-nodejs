using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Module-level events helpers.
/// </summary>
public static class events
{
    private static bool _captureRejections;

    /// <summary>
    /// Symbol-like key used for capture rejection handling.
    /// </summary>
    public static string captureRejectionSymbol => "Tsonic.CSharp.Node.captureRejection";

    /// <summary>
    /// Whether capture rejections are enabled by default.
    /// </summary>
    public static bool captureRejections
    {
        get => _captureRejections;
        set => _captureRejections = value;
    }

    /// <summary>
    /// Default max listeners for new EventEmitter instances.
    /// </summary>
    public static int defaultMaxListeners
    {
        get => EventEmitter.defaultMaxListeners;
        set => EventEmitter.defaultMaxListeners = value;
    }

    /// <summary>
    /// Error-monitor event key.
    /// </summary>
    public static string errorMonitor => "errorMonitor";

    /// <summary>
    /// Adds a listener that can be disposed manually.
    /// </summary>
    public static Action addAbortListener(object signal, Action listener)
    {
        _ = signal;
        return listener ?? throw new ArgumentNullException(nameof(listener));
    }

    /// <summary>
    /// Returns listeners currently attached for an event.
    /// </summary>
    public static Delegate[] getEventListeners(EventEmitter emitter, string eventName)
    {
        if (emitter == null)
            throw new ArgumentNullException(nameof(emitter));

        return emitter.listeners(eventName);
    }

    /// <summary>
    /// Returns current max listener count for an emitter.
    /// </summary>
    public static int getMaxListeners(EventEmitter emitter)
    {
        if (emitter == null)
            throw new ArgumentNullException(nameof(emitter));

        return emitter.getMaxListeners();
    }

    /// <summary>
    /// Returns listener count for a given event.
    /// </summary>
    public static int listenerCount(EventEmitter emitter, string eventName)
    {
        if (emitter == null)
            throw new ArgumentNullException(nameof(emitter));

        return emitter.listenerCount(eventName);
    }

    /// <summary>
    /// Returns an async sequence of emitted event argument arrays.
    /// </summary>
    public static async IAsyncEnumerable<object?[]> on(EventEmitter emitter, string eventName)
    {
        if (emitter == null)
            throw new ArgumentNullException(nameof(emitter));

        while (true)
        {
            yield return await EventEmitter.once(emitter, eventName).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Returns a task that resolves when the next event fires.
    /// </summary>
    public static Task<object?[]> once(EventEmitter emitter, string eventName)
    {
        return EventEmitter.once(emitter, eventName);
    }

    /// <summary>
    /// Sets max listeners on one or more emitters.
    /// </summary>
    public static void setMaxListeners(int n, params EventEmitter[] emitters)
    {
        if (emitters == null)
            return;

        foreach (var emitter in emitters)
        {
            emitter?.setMaxListeners(n);
        }
    }
}
