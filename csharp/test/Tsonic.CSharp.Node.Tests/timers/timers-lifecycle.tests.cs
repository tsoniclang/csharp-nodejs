using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public partial class TimersTests
{
    [Fact]
    public void Timeout_ref_ShouldReturnThis() => JsEventLoop.Run(() =>
    {
        var timeout = timers.setTimeout(() => { }, 100);
        var result = timeout.@ref();
        Assert.Same(timeout, result);
        timers.clearTimeout(timeout);
    });

    [Fact]
    public void Timeout_unref_ShouldReturnThis() => JsEventLoop.Run(() =>
    {
        var timeout = timers.setTimeout(() => { }, 100);
        var result = timeout.unref();
        Assert.Same(timeout, result);
        timers.clearTimeout(timeout);
    });

    [Fact]
    public void Timeout_hasRef_ShouldReturnTrue() => JsEventLoop.Run(() =>
    {
        var timeout = timers.setTimeout(() => { }, 100);
        Assert.True(timeout.hasRef());
        timers.clearTimeout(timeout);
    });

    [Fact]
    public void Timeout_hasRef_AfterUnref_ShouldReturnFalse() => JsEventLoop.Run(() =>
    {
        var timeout = timers.setTimeout(() => { }, 100);
        timeout.unref();
        Assert.False(timeout.hasRef());
        timers.clearTimeout(timeout);
    });

    [Fact]
    public void Timeout_refresh_ShouldReturnThis() => JsEventLoop.Run(() =>
    {
        var timeout = timers.setTimeout(() => { }, 100);
        var result = timeout.refresh();
        Assert.Same(timeout, result);
        timers.clearTimeout(timeout);
    });

    [Fact]
    public void Timeout_refresh_ShouldRestartDelay() => JsEventLoop.Run(async () =>
    {
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var timeout = timers.setTimeout(completed.SetResult, 120);
        try
        {
            await Task.Delay(80);
            timeout.refresh();
            Assert.False(await Task.WhenAny(completed.Task, Task.Delay(60)) == completed.Task, "refresh should restart the original timeout delay");
            Assert.True(await Task.WhenAny(completed.Task, Task.Delay(500)) == completed.Task, "refreshed timeout should eventually execute");
        }
        finally { timers.clearTimeout(timeout); }
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Timeout_refresh_ShouldRetireQueuedDelivery(bool deliverRefreshedTimeout)
    {
        JsEventLoop.Run(async () =>
        {
            var invocations = 0;
            var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var timeout = timers.setTimeout(() =>
            {
                invocations++;
                completed.TrySetResult();
            }, deliverRefreshedTimeout ? 0 : 120);
            try
            {
                QueueTimeoutDelivery(timeout);
                timeout.refresh();
                TimeoutDispatch(timeout).Enqueue(() => Assert.Equal(deliverRefreshedTimeout ? 1 : 0, invocations));
                Assert.True(await Task.WhenAny(completed.Task, Task.Delay(500)) == completed.Task);
                Assert.Equal(1, invocations);
            }
            finally { timers.clearTimeout(timeout); }
        });
    }

    [Fact]
    public void Timeout_refresh_ShouldPreserveLatePhysicalProducerDeadline() => JsEventLoop.Run(async () =>
    {
        var invocations = 0;
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var timeout = timers.setTimeout(() =>
        {
            invocations++;
            completed.TrySetResult();
        }, 120);
        try
        {
            timeout.refresh();
            typeof(Tsonic.CSharp.Node.Timeout).GetMethod("Execute", BindingFlags.Instance | BindingFlags.NonPublic)!
                .Invoke(timeout, null);
            Assert.False(await Task.WhenAny(completed.Task, Task.Delay(60)) == completed.Task);
            Assert.True(await Task.WhenAny(completed.Task, Task.Delay(500)) == completed.Task);
            Assert.Equal(1, invocations);
        }
        finally { timers.clearTimeout(timeout); }
    });

    [Fact]
    public void Timeout_refresh_InsideCallback_ShouldRearmWithoutLosingRegistration() => JsEventLoop.Run(async () =>
    {
        var invocations = 0;
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Tsonic.CSharp.Node.Timeout? timeout = null;
        timeout = timers.setTimeout(() =>
        {
            invocations++;
            if (invocations == 1)
                timeout!.refresh();
            else
                completed.TrySetResult();
        }, 60);
        try
        {
            Assert.True(await Task.WhenAny(completed.Task, Task.Delay(500)) == completed.Task);
            Assert.Equal(2, invocations);
            Assert.False(ProcessKeepAlive.HasReferences);
        }
        finally { timers.clearTimeout(timeout); }
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Timeout_close_InsideCallback_ShouldNotRearm(bool clear)
    {
        var invocations = 0;
        JsEventLoop.Run(() =>
        {
            Tsonic.CSharp.Node.Timeout? timeout = null;
            timeout = timers.setTimeout(() =>
            {
                invocations++;
                if (clear) timers.clearTimeout(timeout);
                else timeout!.close();
                timeout!.refresh();
                Assert.False(ProcessKeepAlive.HasReferences);
            }, 0);
        });
        Assert.Equal(1, invocations);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Timeout_close_ShouldRetireQueuedDelivery(bool clear)
    {
        var invocations = 0;
        JsEventLoop.Run(() =>
        {
            var timeout = timers.setTimeout(() => invocations++, 120);
            QueueTimeoutDelivery(timeout);
            if (clear) timers.clearTimeout(timeout);
            else timeout.close();
            TimeoutDispatch(timeout).Enqueue(() => Assert.Equal(0, invocations));
        });
        Assert.Equal(0, invocations);
    }

    [Fact]
    public void Timeout_refresh_ShouldPreserveIntervalPeriod() => JsEventLoop.Run(async () =>
    {
        var invocations = 0;
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Tsonic.CSharp.Node.Timeout? timeout = null;
        timeout = timers.setInterval(() =>
        {
            invocations++;
            if (invocations == 1) timeout!.refresh();
            Assert.Equal(20, (int)typeof(Tsonic.CSharp.Node.Timeout)
                .GetField("_period", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(timeout)!);
            if (invocations == 3)
            {
                timers.clearInterval(timeout);
                completed.TrySetResult();
            }
        }, 20);
        try
        {
            Assert.True(await Task.WhenAny(completed.Task, Task.Delay(500)) == completed.Task);
            Assert.Equal(3, invocations);
            Assert.False(ProcessKeepAlive.HasReferences);
        }
        finally { timers.clearInterval(timeout); }
    });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Timeout_OwnerRegistration_ShouldRetainDroppedHandleAcrossGc(bool unreferenced) => JsEventLoop.Run(async () =>
    {
        var invocations = 0;
        var completed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handle = CreateDroppedTimeout(() =>
        {
            invocations++;
            completed.TrySetResult();
        }, unreferenced);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Assert.True(handle.TryGetTarget(out _), "the existing loop registration retains its dropped timeout");
        Assert.True(await Task.WhenAny(completed.Task, Task.Delay(500)) == completed.Task);
        Assert.Equal(1, invocations);
        Assert.False(ProcessKeepAlive.HasReferences);
    });

    [Fact]
    public void Timeout_Shutdown_ShouldDisposeAndRetireItsOnlyRegistration()
    {
        System.Collections.IDictionary? registrations = null;
        var originalCount = -1;
        Tsonic.CSharp.Node.Timeout? timeout = null;
        JsEventLoop.Run(() =>
        {
            var owner = typeof(JsEventLoop).GetField("_owner", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
            registrations = (System.Collections.IDictionary)Type.GetType("Tsonic.CSharp.Js.LoopContext, Tsonic.CSharp.Js", throwOnError: true)!
                .GetField("_registrations", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(owner)!;
            originalCount = registrations.Count;
            timeout = timers.setTimeout(() => Assert.Fail("shutdown must cancel this timeout"), 60000).unref();
            Assert.Equal(originalCount + 1, registrations.Count);
        });
        Assert.Equal(originalCount, registrations!.Count);
        Assert.True((bool)typeof(Tsonic.CSharp.Node.Timeout)
            .GetField("_disposed", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(timeout)!);
    }

    [Fact]
    public void Timeout_close_ShouldCancelTimeout()
    {
        var executed = false;
        JsEventLoop.Run(() => timers.setTimeout(() => executed = true, 50).close());
        Thread.Sleep(100);
        JsEventLoop.Run();
        Assert.False(executed);
    }

    [Fact]
    public void Immediate_ref_ShouldReturnThis() => JsEventLoop.Run(() =>
    {
        var immediate = timers.setImmediate(() => { });
        var result = immediate.@ref();
        Assert.Same(immediate, result);
        timers.clearImmediate(immediate);
    });

    [Fact]
    public void Immediate_unref_ShouldReturnThis() => JsEventLoop.Run(() =>
    {
        var immediate = timers.setImmediate(() => { });
        var result = immediate.unref();
        Assert.Same(immediate, result);
        timers.clearImmediate(immediate);
    });

    [Fact]
    public void Immediate_hasRef_ShouldReturnTrue() => JsEventLoop.Run(() =>
    {
        var immediate = timers.setImmediate(() => { });
        Assert.True(immediate.hasRef());
        timers.clearImmediate(immediate);
    });

    [Fact]
    public void Immediate_hasRef_AfterUnref_ShouldReturnFalse() => JsEventLoop.Run(() =>
    {
        var immediate = timers.setImmediate(() => { });
        immediate.unref();
        Assert.False(immediate.hasRef());
        timers.clearImmediate(immediate);
    });

    private static LoopDispatch TimeoutDispatch(Tsonic.CSharp.Node.Timeout timeout) =>
        (LoopDispatch)typeof(Tsonic.CSharp.Node.Timeout)
            .GetProperty("TimerDispatch", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(timeout)!;

    private static void QueueTimeoutDelivery(Tsonic.CSharp.Node.Timeout timeout)
    {
        var callback = (SendOrPostCallback)typeof(Tsonic.CSharp.Node.Timeout)
            .GetField("DispatchCallback", BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
        TimeoutDispatch(timeout).Enqueue(callback, timeout);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<Tsonic.CSharp.Node.Timeout> CreateDroppedTimeout(Action callback, bool unreferenced)
    {
        var timeout = timers.setTimeout(callback, 60);
        if (unreferenced) timeout.unref();
        return new WeakReference<Tsonic.CSharp.Node.Timeout>(timeout);
    }
}
