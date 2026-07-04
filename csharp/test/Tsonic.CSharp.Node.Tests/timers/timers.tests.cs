using Xunit;
using System.Threading;

namespace Tsonic.CSharp.Node.Tests;

public class TimersTests
{
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
    }

    [Fact]
    public void setTimeout_WithZeroDelay_ShouldExecute()
    {
        var resetEvent = new ManualResetEventSlim(false);
        var executed = false;
        timers.setTimeout(() =>
        {
            executed = true;
            resetEvent.Set();
        }, 0);

        var signaled = resetEvent.Wait(1000);
        Assert.True(signaled, "setTimeout(0) callback was not called within timeout");
        Assert.True(executed);
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
        var signaled = resetEvent.Wait(200);

        Assert.False(signaled, "clearTimeout should prevent callback execution");
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
    }

    [Fact]
    public void clearImmediate_ShouldCancelImmediate()
    {
        var executed = false;
        var immediate = timers.setImmediate(() => executed = true);

        timers.clearImmediate(immediate);
        Thread.Sleep(100);

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
            Thread.Sleep(20);

            Assert.False(executed);
        }
    }

    [Fact]
    public void clearImmediate_ShouldCancelImmediate_AtScale()
    {
        for (var index = 0; index < 100; index++)
        {
            var resetEvent = new ManualResetEventSlim(false);
            var executed = false;
            var immediate = timers.setImmediate(() =>
            {
                executed = true;
                resetEvent.Set();
            });

            timers.clearImmediate(immediate);

            var signaled = resetEvent.Wait(20);
            Assert.False(signaled, $"Cancelled immediate executed on iteration {index}");
            Assert.False(executed, $"Cancelled immediate executed on iteration {index}");
        }
    }

    [Fact]
    public void clearImmediate_ShouldCancelAllPendingImmediates()
    {
        var resetEvent = new ManualResetEventSlim(false);
        var executedCount = 0;
        var immediates = new Immediate[32];

        for (var index = 0; index < immediates.Length; index++)
        {
            immediates[index] = timers.setImmediate(() =>
            {
                if (Interlocked.Increment(ref executedCount) == 1)
                {
                    resetEvent.Set();
                }
            });
        }

        for (var index = 0; index < immediates.Length; index++)
        {
            timers.clearImmediate(immediates[index]);
        }

        var signaled = resetEvent.Wait(20);
        Assert.False(signaled, "At least one cancelled immediate executed");
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

        Assert.False(executed);
    }

    [Fact]
    public void Immediate_ref_ShouldReturnThis()
    {
        var immediate = timers.setImmediate(() => { });
        var result = immediate.@ref();

        Assert.Same(immediate, result);
        timers.clearImmediate(immediate);
    }

    [Fact]
    public void Immediate_unref_ShouldReturnThis()
    {
        var immediate = timers.setImmediate(() => { });
        var result = immediate.unref();

        Assert.Same(immediate, result);
        timers.clearImmediate(immediate);
    }

    [Fact]
    public void Immediate_hasRef_ShouldReturnTrue()
    {
        var immediate = timers.setImmediate(() => { });
        Assert.True(immediate.hasRef());
        timers.clearImmediate(immediate);
    }

    [Fact]
    public void Immediate_hasRef_AfterUnref_ShouldReturnFalse()
    {
        var immediate = timers.setImmediate(() => { });
        immediate.unref();
        Assert.False(immediate.hasRef());
        timers.clearImmediate(immediate);
    }
}
