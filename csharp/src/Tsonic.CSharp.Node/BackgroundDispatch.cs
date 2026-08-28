using System;
using System.Threading;
using System.Threading.Tasks;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node;

internal static class BackgroundDispatch
{
    private static readonly TaskFactory Factory = new(
        CancellationToken.None,
        TaskCreationOptions.LongRunning,
        TaskContinuationOptions.None,
        TaskScheduler.Default);

    public static Task RunReferenced(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ProcessKeepAlive.Acquire();
        try
        {
            return Factory.StartNew(() =>
            {
                try
                {
                    action();
                }
                finally
                {
                    ProcessKeepAlive.Release();
                }
            });
        }
        catch
        {
            ProcessKeepAlive.Release();
            throw;
        }
    }

    public static Task RunReferencedAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        ProcessKeepAlive.Acquire();
        try
        {
            return Factory.StartNew(async () =>
            {
                try
                {
                    await action().ConfigureAwait(false);
                }
                finally
                {
                    ProcessKeepAlive.Release();
                }
            }).Unwrap();
        }
        catch
        {
            ProcessKeepAlive.Release();
            throw;
        }
    }

    public static Task RunHandleOwned(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Factory.StartNew(action);
    }

    public static Task RunHandleOwnedAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return Factory.StartNew(action).Unwrap();
    }
}
