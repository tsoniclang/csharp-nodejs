using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public sealed class LoopDispatchOwnerTests
{
    [Fact]
    public void NativeModuleDispatchRequiresAnExplicitActiveOwner()
    {
        var failure = Assert.Throws<InvalidOperationException>(() => BackgroundDispatch.Capture(NodePhases.Net));
        Assert.Equal("No JavaScript event loop is running on this thread.", failure.Message);
        Assert.Throws<InvalidOperationException>(() => timers.setTimeout(static () => { }, 1));
        Assert.Throws<InvalidOperationException>(() => new Worker("missing-native-owner"));
    }

    [Fact]
    public void NativeStreamCapacityFailureOccursBeforeFileMutationOrHandleRetention()
    {
        var path = System.IO.Path.Combine(AppContext.BaseDirectory, "owner-admission-" + Guid.NewGuid() + ".txt");
        System.IO.File.WriteAllText(path, "unchanged");
        try
        {
            JsEventLoop.Run(() =>
            {
                var dispatch = JsEventLoop.Capture();
                for (var index = 0; index < 16384; index++) dispatch.Register(static () => { });
                Assert.Throws<InvalidOperationException>(() => new ReadStream(path));
                Assert.Throws<InvalidOperationException>(() => new WriteStream(path));
                Assert.Equal("unchanged", System.IO.File.ReadAllText(path));
                using var exclusive = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.ReadWrite, System.IO.FileShare.None);
                Assert.True(exclusive.CanWrite);
            });
        }
        finally { System.IO.File.Delete(path); }
    }

    [Fact]
    public void NodeOwnsOrderedPhaseTokensAndKeepsJavascriptReactionsFirst()
    {
        var seen = new List<int>();
        JsEventLoop.Run(() =>
        {
            var ports = BackgroundDispatch.Capture(NodePhases.Ports);
            var sockets = BackgroundDispatch.Capture(NodePhases.Net);
            var timers = BackgroundDispatch.Capture(NodePhases.Timers);
            var background = BackgroundDispatch.Capture(NodePhases.Background);
            BackgroundDispatch.Enqueue(ports, () => seen.Add(4));
            BackgroundDispatch.Enqueue(sockets, () => seen.Add(3));
            BackgroundDispatch.Enqueue(timers, () => seen.Add(2));
            BackgroundDispatch.Enqueue(background, () => seen.Add(1));
            JsEventLoop.Enqueue(() => seen.Add(0));
        });
        Assert.Equal(new[] { 0, 1, 2, 3, 4 }, seen);
    }

    [Fact]
    public void ClosedNativeHandlesCannotRebindOrPostIntoTheNextGeneration()
    {
        var dispatch = default(LoopDispatch);
        var called = false;
        JsEventLoop.Run(() => { dispatch = BackgroundDispatch.Capture(NodePhases.Net); });
        JsEventLoop.Run(() =>
        {
            var rebound = BackgroundDispatch.Bind(ref dispatch, NodePhases.Net);
            Assert.True(rebound.IsBound);
            Assert.False(rebound.IsActive);
            Assert.False(rebound.TryEnqueue(() => called = true));
            Assert.Throws<InvalidOperationException>(rebound.Acquire);
            rebound.Release();
            BackgroundDispatch.Enqueue(BackgroundDispatch.Capture(NodePhases.Net), static () => { });
        });
        Assert.False(called);
    }

    [Fact]
    public void WarmedNodePhaseAdmissionHasNoAllocationsOrPerJobReferences()
    {
        var called = 0;
        Action callback = () => called++;
        void Warm()
        {
            var dispatch = BackgroundDispatch.Capture(NodePhases.Net);
            for (var index = 0; index < 4096; index++) BackgroundDispatch.Enqueue(dispatch, callback);
        }
        JsEventLoop.Run(Warm);
        var allocated = -1L;
        JsEventLoop.Run(() =>
        {
            var dispatch = BackgroundDispatch.Capture(NodePhases.Net);
            var before = GC.GetTotalAllocatedBytes(true);
            for (var index = 0; index < 4096; index++) BackgroundDispatch.Enqueue(dispatch, callback);
            allocated = GC.GetTotalAllocatedBytes(true) - before;
            Assert.False(ProcessKeepAlive.HasReferences);
        });
        Assert.Equal(8192, called);
        Assert.Equal(0, allocated);
    }

    [Fact]
    public void ShutdownDetachesUnreferencedNativeTimersAndMessagePorts()
    {
        MessageChannel? channel = null;
        Timeout? timer = null;
        var invoked = false;
        var portCalls = 0;
        JsEventLoop.Run(() =>
        {
            channel = new MessageChannel();
            channel.port2.on<Tsonic.CSharp.Runtime.TsValue>("message", _ => portCalls++);
            timer = timers.setTimeout(() => invoked = true, 60000).unref();
        });
        Assert.NotNull(channel);
        Assert.NotNull(timer);
        JsEventLoop.Run(() =>
        {
            timer.refresh().@ref();
            Assert.False(ProcessKeepAlive.HasReferences);
            channel.port1.postMessage(Tsonic.CSharp.Runtime.TsValue.from(9_007_199_254_740_993L));
            Assert.Equal(9_007_199_254_740_993L,
                Assert.IsType<long>(channel.port2.receiveMessageOnPort().unwrap()));
            timers.setImmediate(static () => { });
        });
        Assert.False(invoked);
        Assert.Equal(0, portCalls);
        Assert.Equal(0, channel.port2.listenerCount("message"));
        JsEventLoop.Run(() =>
        {
            channel.port2.on<Tsonic.CSharp.Runtime.TsValue>("message", value =>
                portCalls += Assert.IsType<int>(value.unwrap()));
            channel.port1.postMessage(Tsonic.CSharp.Runtime.TsValue.from(3));
        });
        Assert.Equal(3, portCalls);
        channel.port1.close();
        channel.port2.close();
    }

    [Fact]
    public void ReferencedBackgroundCompletionPrecedesTheResourceRelease()
    {
        var called = false;
        var owner = Environment.CurrentManagedThreadId;
        JsEventLoop.Run(() =>
        {
            var source = BackgroundDispatch.RunReferencedAsync(static () => Task.CompletedTask);
            _ = PromiseRuntime.Then(source, () =>
            {
                Assert.Equal(owner, Environment.CurrentManagedThreadId);
                called = true;
            });
        });
        Assert.True(called);
    }
}
