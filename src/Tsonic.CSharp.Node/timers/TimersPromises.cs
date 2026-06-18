using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Promise-based helpers for the timers module.
/// </summary>
public class TimersPromises
{
    private static readonly TimersScheduler _scheduler = new();

    /// <summary>
    /// Scheduler helper object.
    /// </summary>
    public TimersScheduler scheduler => _scheduler;

    /// <summary>
    /// Promise-based setTimeout.
    /// </summary>
    public async Task<object?> setTimeout(int delay = 1, object? value = null)
    {
        await Task.Delay(delay < 0 ? 0 : delay).ConfigureAwait(false);
        return value;
    }

    /// <summary>
    /// Promise-based setImmediate.
    /// </summary>
    public async Task<object?> setImmediate(object? value = null)
    {
        await Task.Yield();
        return value;
    }

    /// <summary>
    /// Promise-based setInterval as async sequence.
    /// </summary>
    public async IAsyncEnumerable<object?> setInterval(int delay = 1, object? value = null)
    {
        var actualDelay = delay < 0 ? 0 : delay;
        while (true)
        {
            await Task.Delay(actualDelay).ConfigureAwait(false);
            yield return value;
        }
    }
}

/// <summary>
/// Scheduler helpers for timers promises.
/// </summary>
public class TimersScheduler
{
    /// <summary>
    /// Waits for the given delay in milliseconds.
    /// </summary>
    public Task wait(int delay = 1)
    {
        return Task.Delay(delay < 0 ? 0 : delay);
    }

    /// <summary>
    /// Yields execution to the scheduler.
    /// </summary>
    public async Task @yield()
    {
        await Task.Yield();
    }
}
