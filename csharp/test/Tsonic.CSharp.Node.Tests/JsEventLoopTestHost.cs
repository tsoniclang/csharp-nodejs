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

    private JsEventLoopTestHost(Action cleanup)
    {
        _cleanup = cleanup;
        _eventLoop = Task.Factory.StartNew(
            JsEventLoop.Run,
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

        using var started = new ManualResetEventSlim();
        JsEventLoop.EnqueueReferenced(started.Set);
        if (!started.Wait(ShutdownTimeout))
            throw new TimeoutException("The test JavaScript event loop did not start.");
    }

    public static JsEventLoopTestHost Start(Action cleanup) => new(cleanup);

    public void Dispose()
    {
        _cleanup();
        if (!_eventLoop.Wait(ShutdownTimeout))
            throw new TimeoutException("The test JavaScript event loop did not stop.");
        _eventLoop.GetAwaiter().GetResult();
    }
}
