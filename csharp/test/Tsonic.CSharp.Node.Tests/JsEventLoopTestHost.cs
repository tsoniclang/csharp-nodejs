using System;
using System.Threading;
using System.Threading.Tasks;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node.Tests;

internal sealed class JsEventLoopTestHost : IDisposable
{
    private static readonly TimeSpan ShutdownTimeout = TimeSpan.FromSeconds(5);
    private readonly Action _cleanup;
    private readonly Task _eventLoop;
    private readonly TaskCompletionSource _lifetime = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _disposed;

    private JsEventLoopTestHost(Action cleanup)
    {
        _cleanup = cleanup;
        using var started = new ManualResetEventSlim();
        JsEventLoop.EnqueueHandleOwned(started.Set);
        _eventLoop = Task.Factory.StartNew(
            () => JsEventLoop.Run(_lifetime.Task),
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

        if (!started.Wait(ShutdownTimeout))
        {
            _lifetime.TrySetResult();
            throw new TimeoutException("The test JavaScript event loop did not start.");
        }
    }

    public static JsEventLoopTestHost Start(Action cleanup) => new(cleanup);

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;
        try
        {
            _cleanup();
        }
        finally
        {
            _lifetime.TrySetResult();
        }
        if (!_eventLoop.Wait(ShutdownTimeout))
            throw new TimeoutException("The test JavaScript event loop did not stop.");
        _eventLoop.GetAwaiter().GetResult();
    }
}
