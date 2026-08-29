using Xunit;
using System.Threading;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class TimersTests
{
    private static void DrainEventLoop()
    {
        JsEventLoop.EnqueueReferenced(static () => { });
        JsEventLoop.Run();
    }

    [Fact]
    public void setTimeout_ShouldExecuteCallback()
    {
        var resetEvent = new ManualResetEventSlim(false);
        var executed = false;
        var timeout = timers.setTimeout(() =>
        {
            executed = true;
            resetEvent.Set();
        }, 50);
        using var eventLoop = JsEventLoopTestHost.Start(() => timers.clearTimeout(timeout));

        var signaled = resetEvent.Wait(1000);
        Assert.True(signaled, "setTimeout callback was not called within timeout");
        Assert.True(executed);
    }

    [Fact]
    public void setTimeout_ShouldReturnTimeout()
    {
        var timeout = timers.setTimeout(() => { }, 10);
        Assert.NotNull(timeout);
        Assert.IsType<Timeout>(timeout);
        timers.clearTimeout(timeout);
    }

    [Fact]
    public void setTimeout_WithZeroDelay_ShouldExecute()
    {
        var resetEvent = new ManualResetEventSlim(false);
        var executed = false;
        var timeout = timers.setTimeout(() =>
        {
            executed = true;
            resetEvent.Set();
        }, 0);
        using var eventLoop = JsEventLoopTestHost.Start(() => timers.clearTimeout(timeout));

        var signaled = resetEvent.Wait(1000);
        Assert.True(signaled, "setTimeout(0) callback was not called within timeout");
        Assert.True(executed);
    }

    [Fact]
    public void setTimeout_WithZeroDelay_ArmsOnlyAfterHandleInitialization()
    {
        const int timeoutCount = 256;
        using var completed = new CountdownEvent(timeoutCount);
        var timeouts = new Timeout[timeoutCount];

        for (var index = 0; index < timeouts.Length; index++)
        {
            timeouts[index] = timers.setTimeout(() => completed.Signal(), 0);
        }
        using var eventLoop = JsEventLoopTestHost.Start(() =>
        {
            foreach (var timeout in timeouts)
                timers.clearTimeout(timeout);
        });

        Assert.True(completed.Wait(5000), "Every zero-delay timeout should execute after its handle is fully initialized");
    }

    [Fact]
    public void clearTimeout_ShouldCancelTimeout()
    {
        var resetEvent = new ManualResetEventSlim(false);
        var executed = false;
        var timeout = timers.setTimeout(() =>
        {
            executed = true;
            resetEvent.Set();
        }, 50);

        timers.clearTimeout(timeout);
        Thread.Sleep(100);
        DrainEventLoop();

        Assert.False(resetEvent.IsSet, "clearTimeout should prevent callback execution");
        Assert.False(executed);
    }

    [Fact]
    public void clearTimeout_WithNull_ShouldNotThrow()
    {
        timers.clearTimeout(null);
        // Should not throw
    }

    [Fact]
    public void setInterval_ShouldExecuteRepeatedlyAsync()
    {
        var count = 0;
        var resetEvent = new ManualResetEventSlim(false);
        var timeout = timers.setInterval(() =>
        {
            if (Interlocked.Increment(ref count) >= 3)
            {
                resetEvent.Set();
            }
        }, 50);
        using var eventLoop = JsEventLoopTestHost.Start(() => timers.clearInterval(timeout));

        var signaled = resetEvent.Wait(2000);
        timers.clearInterval(timeout);

        Assert.True(signaled, $"Expected at least 3 executions, got {count}");
        Assert.True(count >= 3, $"Expected at least 3 executions, got {count}");
    }

    [Fact]
    public void clearInterval_ShouldNotThrow()
    {
        var count = 0;
        var resetEvent = new ManualResetEventSlim(false);
        var timeout = timers.setInterval(() =>
        {
            Interlocked.Increment(ref count);
            resetEvent.Set();
        }, 50);
        using var eventLoop = JsEventLoopTestHost.Start(() => timers.clearInterval(timeout));

        var signaled = resetEvent.Wait(5000);
        timers.clearInterval(timeout);

        Assert.True(signaled, "Interval should have executed at least once");
        // Just verify that clearInterval doesn't throw
        Assert.True(count > 0, "Interval should have executed at least once");
    }

    [Fact]
    public void setImmediate_ShouldExecuteCallback()
    {
        var resetEvent = new ManualResetEventSlim(false);
        var executed = false;
        var immediate = timers.setImmediate(() =>
        {
            executed = true;
            resetEvent.Set();
        });
        using var eventLoop = JsEventLoopTestHost.Start(() => timers.clearImmediate(immediate));

        var signaled = resetEvent.Wait(1000);
        Assert.True(signaled, "setImmediate callback was not called within timeout");
        Assert.True(executed);
    }

    [Fact]
    public void setImmediate_ShouldExecuteCallback_Reliably()
    {
        for (var index = 0; index < 10; index++)
        {
            var resetEvent = new ManualResetEventSlim(false);
            var executed = false;

            var immediate = timers.setImmediate(() =>
            {
                executed = true;
                resetEvent.Set();
            });
            using var eventLoop = JsEventLoopTestHost.Start(() => timers.clearImmediate(immediate));

            try
            {
                var signaled = resetEvent.Wait(1000);
                Assert.True(signaled, $"setImmediate callback was not called within timeout on iteration {index}");
                Assert.True(executed);
            }
            finally
            {
                timers.clearImmediate(immediate);
            }
        }
    }

    [Fact]
    public void setImmediate_ShouldReturnImmediate()
    {
        var immediate = timers.setImmediate(() => { });
        Assert.NotNull(immediate);
        Assert.IsType<Immediate>(immediate);
        timers.clearImmediate(immediate);
        DrainEventLoop();
    }

    [Fact]
    public void clearImmediate_ShouldCancelImmediate()
    {
        var executed = false;
        var immediate = timers.setImmediate(() => executed = true);
        timers.clearImmediate(immediate);
        DrainEventLoop();

        Assert.False(executed);
    }

    [Fact]
    public void clearImmediate_ShouldCancelImmediate_Reliably()
    {
        for (var index = 0; index < 10; index++)
        {
            var executed = false;
            var immediate = timers.setImmediate(() => executed = true);
            timers.clearImmediate(immediate);
            DrainEventLoop();

            Assert.False(executed);
        }
    }

    [Fact]
    public void clearImmediate_ShouldCancelImmediate_AtScale()
    {
        var executedCount = 0;
        var immediates = new Immediate[100];
        for (var index = 0; index < immediates.Length; index++)
        {
            immediates[index] = timers.setImmediate(() => Interlocked.Increment(ref executedCount));
        }

        foreach (var immediate in immediates)
            timers.clearImmediate(immediate);
        DrainEventLoop();

        Assert.Equal(0, executedCount);
    }

    [Fact]
    public void clearImmediate_ShouldCancelAllPendingImmediates()
    {
        var executedCount = 0;
        var immediates = new Immediate[32];
        for (var index = 0; index < immediates.Length; index++)
        {
            immediates[index] = timers.setImmediate(() => Interlocked.Increment(ref executedCount));
        }

        foreach (var immediate in immediates)
            timers.clearImmediate(immediate);
        DrainEventLoop();

        Assert.Equal(0, executedCount);
    }

    [Fact]
    public void clearImmediate_WithNull_ShouldNotThrow()
    {
        timers.clearImmediate(null);
        // Should not throw
    }

    [Fact]
    public void queueMicrotask_ShouldExecuteCallback()
    {
        var resetEvent = new ManualResetEventSlim(false);
        var executed = false;
        timers.queueMicrotask(() =>
        {
            executed = true;
            resetEvent.Set();
        });
        using var eventLoop = JsEventLoopTestHost.Start(static () => { });

        var signaled = resetEvent.Wait(1000);
        Assert.True(signaled, "queueMicrotask callback was not called within timeout");
        Assert.True(executed);
    }

    [Fact]
    public void Timeout_ref_ShouldReturnThis()
    {
        var timeout = timers.setTimeout(() => { }, 100);
        var result = timeout.@ref();

        Assert.Same(timeout, result);
        timers.clearTimeout(timeout);
    }

    [Fact]
    public void Timeout_unref_ShouldReturnThis()
    {
        var timeout = timers.setTimeout(() => { }, 100);
        var result = timeout.unref();

        Assert.Same(timeout, result);
        timers.clearTimeout(timeout);
    }

    [Fact]
    public void Timeout_hasRef_ShouldReturnTrue()
    {
        var timeout = timers.setTimeout(() => { }, 100);
        Assert.True(timeout.hasRef());
        timers.clearTimeout(timeout);
    }

    [Fact]
    public void Timeout_hasRef_AfterUnref_ShouldReturnFalse()
    {
        var timeout = timers.setTimeout(() => { }, 100);
        timeout.unref();
        Assert.False(timeout.hasRef());
        timers.clearTimeout(timeout);
    }

    [Fact]
    public void Timeout_refresh_ShouldReturnThis()
    {
        var timeout = timers.setTimeout(() => { }, 100);
        var result = timeout.refresh();

        Assert.Same(timeout, result);
        timers.clearTimeout(timeout);
    }

    [Fact]
    public void Timeout_refresh_ShouldRestartDelay()
    {
        var resetEvent = new ManualResetEventSlim(false);
        var timeout = timers.setTimeout(() => resetEvent.Set(), 120);
        using var eventLoop = JsEventLoopTestHost.Start(() => timers.clearTimeout(timeout));

        Thread.Sleep(80);
        timeout.refresh();

        Assert.False(resetEvent.Wait(60), "refresh should restart the original timeout delay");
        Assert.True(resetEvent.Wait(500), "refreshed timeout should eventually execute");
    }

    [Fact]
    public void Timeout_close_ShouldCancelTimeout()
    {
        var executed = false;
        var timeout = timers.setTimeout(() => executed = true, 50);

        timeout.close();
        Thread.Sleep(100);
        DrainEventLoop();

        Assert.False(executed);
    }

    [Fact]
    public void Immediate_ref_ShouldReturnThis()
    {
        var immediate = timers.setImmediate(() => { });
        var result = immediate.@ref();

        Assert.Same(immediate, result);
        timers.clearImmediate(immediate);
        DrainEventLoop();
    }

    [Fact]
    public void Immediate_unref_ShouldReturnThis()
    {
        var immediate = timers.setImmediate(() => { });
        var result = immediate.unref();

        Assert.Same(immediate, result);
        timers.clearImmediate(immediate);
        DrainEventLoop();
    }

    [Fact]
    public void Immediate_hasRef_ShouldReturnTrue()
    {
        var immediate = timers.setImmediate(() => { });
        Assert.True(immediate.hasRef());
        timers.clearImmediate(immediate);
        DrainEventLoop();
    }

    [Fact]
    public void Immediate_hasRef_AfterUnref_ShouldReturnFalse()
    {
        var immediate = timers.setImmediate(() => { });
        immediate.unref();
        Assert.False(immediate.hasRef());
        timers.clearImmediate(immediate);
        DrainEventLoop();
    }
}
