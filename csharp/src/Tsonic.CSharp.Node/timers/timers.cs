using System.Threading;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Timer functions for scheduling code execution.
/// </summary>
public static class timers
{
    private static readonly TimersPromises _promises = new();

    /// <summary>
    /// Promise-based timer APIs.
    /// </summary>
    public static TimersPromises promises => _promises;

    /// <summary>
    /// Schedules execution of a one-time callback after delay milliseconds.
    /// </summary>
    /// <param name="callback">The function to call when the timer elapses.</param>
    /// <param name="delay">The number of milliseconds to wait before calling callback.</param>
    /// <returns>A Timeout object that can be used with clearTimeout().</returns>
    public static Timeout setTimeout(Action callback, int delay = 0)
    {
        return new Timeout(callback, System.Math.Max(0, delay));
    }

    /// <summary>
    /// Cancels a Timeout object created by setTimeout().
    /// </summary>
    /// <param name="timeout">A Timeout object as returned by setTimeout().</param>
    public static void clearTimeout(Timeout? timeout)
    {
        timeout?.Dispose();
    }

    /// <summary>
    /// Schedules repeated execution of callback every delay milliseconds.
    /// </summary>
    /// <param name="callback">The function to call when the timer elapses.</param>
    /// <param name="delay">The number of milliseconds to wait before each call to callback.</param>
    /// <returns>A Timeout object that can be used with clearInterval().</returns>
    public static Timeout setInterval(Action callback, int delay = 0)
    {
        var actualDelay = System.Math.Max(0, delay);
        return new Timeout(callback, actualDelay, actualDelay);
    }

    /// <summary>
    /// Cancels a Timeout object created by setInterval().
    /// </summary>
    /// <param name="timeout">A Timeout object as returned by setInterval().</param>
    public static void clearInterval(Timeout? timeout)
    {
        timeout?.Dispose();
    }

    /// <summary>
    /// Schedules the immediate execution of callback after I/O events callbacks.
    /// </summary>
    /// <param name="callback">The function to call at the end of this turn of the event loop.</param>
    /// <returns>An Immediate object that can be used with clearImmediate().</returns>
    public static Immediate setImmediate(Action callback)
    {
        return new Immediate(callback);
    }

    /// <summary>
    /// Cancels an Immediate object created by setImmediate().
    /// </summary>
    /// <param name="immediate">An Immediate object as returned by setImmediate().</param>
    public static void clearImmediate(Immediate? immediate)
    {
        immediate?.Dispose();
    }

    /// <summary>
    /// Queues a microtask to invoke callback.
    /// </summary>
    /// <param name="callback">The function to call.</param>
    public static void queueMicrotask(Action callback)
    {
        ProcessKeepAlive.Acquire();
        var thread = new Thread(() =>
        {
            try
            {
                callback();
            }
            finally
            {
                ProcessKeepAlive.Release();
            }
        })
        {
            IsBackground = true,
            Name = "Tsonic.CSharp.Node.Microtask",
        };
        thread.Start();
    }
}
