using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591
#pragma warning disable IDE1006

public static class timers_promises
{
    public static TimersScheduler scheduler => timers.promises.scheduler;

    public static Task<object?> setTimeout(int delay = 1, object? value = null)
    {
        return timers.promises.setTimeout(delay, value);
    }

    public static Task<object?> setImmediate(object? value = null)
    {
        return timers.promises.setImmediate(value);
    }

    public static IAsyncEnumerable<object?> setInterval(int delay = 1, object? value = null)
    {
        return timers.promises.setInterval(delay, value);
    }
}
