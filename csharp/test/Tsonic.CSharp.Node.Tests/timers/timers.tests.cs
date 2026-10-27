using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public partial class TimersTests
{
    [Fact]
    public void EventLoopHost_StartsWithoutPendingTimers()
    {
        for (var index = 0; index < 32; index++)
            JsEventLoop.Run(() => Assert.False(ProcessKeepAlive.HasReferences));
    }

    [Fact]
    public void setTimeout_ShouldExecuteCallback()
    {
        using var resetEvent = new ManualResetEventSlim(false);
        var executed = false;
        var owner = Environment.CurrentManagedThreadId;
        JsEventLoop.Run(() => timers.setTimeout(() =>
        {
            Assert.Equal(owner, Environment.CurrentManagedThreadId);
            executed = true;
            resetEvent.Set();
        }, 50));
        Assert.True(resetEvent.Wait(1000), "setTimeout callback was not called within timeout");
        Assert.True(executed);
    }

    [Fact]
    public void setTimeout_ShouldReturnTimeout() => JsEventLoop.Run(() =>
    {
        var timeout = timers.setTimeout(() => { }, 10);
        Assert.NotNull(timeout);
        Assert.IsType<Timeout>(timeout);
        timers.clearTimeout(timeout);
    });

    [Fact]
    public void setTimeout_WithZeroDelay_ShouldExecute()
    {
        using var resetEvent = new ManualResetEventSlim(false);
        var executed = false;
        JsEventLoop.Run(() => timers.setTimeout(() =>
        {
            executed = true;
            resetEvent.Set();
        }, 0));
        Assert.True(resetEvent.Wait(1000), "setTimeout(0) callback was not called within timeout");
        Assert.True(executed);
    }

    [Fact]
    public void setTimeout_WithZeroDelay_ArmsOnlyAfterHandleInitialization()
    {
        const int timeoutCount = 256;
        using var completed = new CountdownEvent(timeoutCount);
        var timeouts = new Timeout[timeoutCount];
        JsEventLoop.Run(() =>
        {
            for (var index = 0; index < timeouts.Length; index++)
                timeouts[index] = timers.setTimeout(() => completed.Signal(), 0);
        });
        Assert.True(completed.Wait(5000), "Every zero-delay timeout should execute after its handle is fully initialized");
    }

    [Fact]
    public void clearTimeout_ShouldCancelTimeout()
    {
        using var resetEvent = new ManualResetEventSlim(false);
        var executed = false;
        JsEventLoop.Run(() =>
        {
            var timeout = timers.setTimeout(() =>
            {
                executed = true;
                resetEvent.Set();
            }, 50);
            timers.clearTimeout(timeout);
        });
        Thread.Sleep(100);
        JsEventLoop.Run();
        Assert.False(resetEvent.IsSet, "clearTimeout should prevent callback execution");
        Assert.False(executed);
    }

    [Fact]
    public void clearTimeout_WithNull_ShouldNotThrow() => timers.clearTimeout(null);

    [Fact]
    public void setInterval_ShouldExecuteRepeatedlyAsync() => JsEventLoop.Run(async () =>
    {
        var count = 0;
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var timeout = timers.setInterval(() =>
        {
            if (Interlocked.Increment(ref count) >= 3) completed.TrySetResult();
        }, 50);
        try
        {
            var signaled = await Task.WhenAny(completed.Task, Task.Delay(2000)) == completed.Task;
            Assert.True(signaled, $"Expected at least 3 executions, got {count}");
            Assert.True(count >= 3, $"Expected at least 3 executions, got {count}");
        }
        finally { timers.clearInterval(timeout); }
    });

    [Fact]
    public void clearInterval_ShouldNotThrow() => JsEventLoop.Run(async () =>
    {
        var count = 0;
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var timeout = timers.setInterval(() =>
        {
            Interlocked.Increment(ref count);
            completed.TrySetResult();
        }, 50);
        try
        {
            var signaled = await Task.WhenAny(completed.Task, Task.Delay(5000)) == completed.Task;
            timers.clearInterval(timeout);
            Assert.True(signaled, "Interval should have executed at least once");
            Assert.True(count > 0, "Interval should have executed at least once");
        }
        finally { timers.clearInterval(timeout); }
    });

    [Fact]
    public void setImmediate_ShouldExecuteCallback()
    {
        using var resetEvent = new ManualResetEventSlim(false);
        var executed = false;
        JsEventLoop.Run(() => timers.setImmediate(() =>
        {
            executed = true;
            resetEvent.Set();
        }));
        Assert.True(resetEvent.Wait(1000), "setImmediate callback was not called within timeout");
        Assert.True(executed);
    }

    [Fact]
    public void setImmediate_ShouldExecuteCallback_Reliably()
    {
        for (var index = 0; index < 10; index++)
        {
            using var resetEvent = new ManualResetEventSlim(false);
            var executed = false;
            JsEventLoop.Run(() => timers.setImmediate(() =>
            {
                executed = true;
                resetEvent.Set();
            }));
            Assert.True(resetEvent.Wait(1000), $"setImmediate callback was not called within timeout on iteration {index}");
            Assert.True(executed);
        }
    }

    [Fact]
    public void setImmediate_ShouldReturnImmediate() => JsEventLoop.Run(() =>
    {
        var immediate = timers.setImmediate(() => { });
        Assert.NotNull(immediate);
        Assert.IsType<Immediate>(immediate);
        timers.clearImmediate(immediate);
    });

    [Fact]
    public void clearImmediate_ShouldCancelImmediate()
    {
        var executed = false;
        JsEventLoop.Run(() =>
        {
            var immediate = timers.setImmediate(() => executed = true);
            timers.clearImmediate(immediate);
        });
        Assert.False(executed);
    }

    [Fact]
    public void clearImmediate_ShouldCancelImmediate_Reliably()
    {
        for (var index = 0; index < 10; index++)
        {
            var executed = false;
            JsEventLoop.Run(() =>
            {
                var immediate = timers.setImmediate(() => executed = true);
                timers.clearImmediate(immediate);
            });
            Assert.False(executed);
        }
    }

    [Fact]
    public void clearImmediate_ShouldCancelImmediate_AtScale()
    {
        var executedCount = 0;
        JsEventLoop.Run(() =>
        {
            var immediates = new Immediate[100];
            for (var index = 0; index < immediates.Length; index++)
                immediates[index] = timers.setImmediate(() => Interlocked.Increment(ref executedCount));
            foreach (var immediate in immediates) timers.clearImmediate(immediate);
        });
        Assert.Equal(0, executedCount);
    }

    [Fact]
    public void clearImmediate_ShouldCancelAllPendingImmediates()
    {
        var executedCount = 0;
        JsEventLoop.Run(() =>
        {
            var immediates = new Immediate[32];
            for (var index = 0; index < immediates.Length; index++)
                immediates[index] = timers.setImmediate(() => Interlocked.Increment(ref executedCount));
            foreach (var immediate in immediates) timers.clearImmediate(immediate);
        });
        Assert.Equal(0, executedCount);
    }

    [Fact]
    public void clearImmediate_WithNull_ShouldNotThrow() => timers.clearImmediate(null);

    [Fact]
    public void queueMicrotask_ShouldExecuteCallback()
    {
        using var resetEvent = new ManualResetEventSlim(false);
        var executed = false;
        JsEventLoop.Run(() => timers.queueMicrotask(() =>
        {
            executed = true;
            resetEvent.Set();
        }));
        Assert.True(resetEvent.Wait(1000), "queueMicrotask callback was not called within timeout");
        Assert.True(executed);
    }

}
