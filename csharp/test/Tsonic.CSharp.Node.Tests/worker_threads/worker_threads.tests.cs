using System.Collections.Generic;
using Tsonic.CSharp.Js;
using Tsonic.CSharp.Node;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class WorkerThreadsTests
{
    [Fact]
    public void MessageChannel_PreservesNativeNumericCarriersAndBits()
    {
        var first = new MessageChannel();
        using var firstPort = first.port1;
        using var secondPort = first.port2;
        object[] values = [(byte)255, (sbyte)-128, short.MinValue, ushort.MaxValue,
            int.MinValue, uint.MaxValue, 9_007_199_254_740_993L, ulong.MaxValue,
            nint.MinValue, nuint.MaxValue, System.Int128.MinValue, System.UInt128.MaxValue,
            System.Half.MaxValue, float.Epsilon, double.Epsilon, decimal.MaxValue, -0.0];
        foreach (var value in values)
        {
            first.port1.postMessage(TsValue.from(value));
            var received = first.port2.receiveMessageOnPort().unwrap();
            Assert.IsType(value.GetType(), received);
            Assert.Equal(value, received);
        }
        JsEventLoop.Run();
    }

    [Fact]
    public void MessageChannel_DeliversMessagesBetweenPorts()
    {
        var channel = new MessageChannel();
        var messages = new List<string>();
        JsEventLoop.Run(() =>
        {
            channel.port2.on<TsValue>("message", value =>
                messages.Add(TsValue.CastDynamic<string>(value)));
            channel.port1.postMessage(TsValue.from("hello"));
        });

        Assert.Equal(["hello"], messages);

        channel.port1.postMessage(TsValue.from("direct"));
        Assert.Equal(
            "direct",
            TsValue.CastDynamic<string>(channel.port2.receiveMessageOnPort()));
        JsEventLoop.Run();
    }

    [Fact]
    public void SharedMessagePort_PreservesInterleavedRegistrationAcrossAliases()
    {
        var channel = new MessageChannel();
        var first = channel.port2;
        var second = channel.port2;
        var order = new List<int>();
        JsEventLoop.Run(() =>
        {
            first.on<TsValue>("message", _ => order.Add(1));
            second.on<TsValue>("message", _ => order.Add(2));
            first.on<TsValue>("message", _ => order.Add(3));
            channel.port1.postMessage(TsValue.from(1));
            channel.port1.postMessage(TsValue.from(2));
        });
        Assert.Equal([1, 2, 3, 1, 2, 3], order);
        Assert.True(channel.port2.receiveMessageOnPort().isUndefined());
        channel.port1.close();
        channel.port2.close();
        JsEventLoop.Run();
    }

    [Fact]
    public void SharedMessagePort_CancellationPreservesCurrentSnapshotOnly()
    {
        var channel = new MessageChannel();
        var first = channel.port2;
        var second = channel.port2;
        var order = new List<int>();
        Action<TsValue> canceled = _ => order.Add(2);
        JsEventLoop.Run(() =>
        {
            first.on<TsValue>("message", _ =>
            {
                order.Add(1);
                first.off("message", canceled);
                Assert.Equal(1, first.listenerCount("message"));
            });
            second.on("message", canceled);
            channel.port1.postMessage(TsValue.from(1));
            channel.port1.postMessage(TsValue.from(2));
        });
        Assert.Equal([1, 2, 1], order);
        Assert.Equal(0, first.listenerCount("message"));
        channel.port1.close();
        channel.port2.close();
        JsEventLoop.Run();
    }

    [Fact]
    public void SharedMessagePort_OriginalFailureRetiresCallbacksAndKeepsNextNativeMessage()
    {
        var channel = new MessageChannel();
        var first = channel.port2;
        var second = channel.port2;
        var original = new InvalidOperationException("original shared port failure");
        var calls = 0;
        WeakReference? capture = null;
        Assert.Same(original, Assert.Throws<InvalidOperationException>(() => JsEventLoop.Run(() =>
        {
            first.once<TsValue>("message", _ =>
            {
                Assert.Equal(1, first.listenerCount("message"));
                throw original;
            });
            capture = RegisterCapturedListener(second, () => calls++);
            channel.port1.postMessage(TsValue.from(1));
            channel.port1.postMessage(TsValue.from(2));
        })));
        Assert.Equal(0, calls);
        Assert.Equal(0, first.listenerCount("message"));
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Assert.NotNull(capture);
        Assert.False(capture.IsAlive);
        JsEventLoop.Run();
        Assert.Equal(0, calls);
        JsEventLoop.Run(() => second.once<TsValue>("message", value =>
        {
            Assert.Equal(2, Assert.IsType<int>(value.unwrap()));
            calls++;
        }));
        Assert.Equal(1, calls);
        Assert.Equal(0, second.listenerCount("message"));
        channel.port1.close();
        channel.port2.close();
        JsEventLoop.Run();
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static WeakReference RegisterCapturedListener(MessagePort port, Action callback)
    {
        var captured = new object();
        port.once<TsValue>("message", _ =>
        {
            GC.KeepAlive(captured);
            callback();
        });
        return new WeakReference(captured);
    }

    [Fact]
    public void EnvironmentData_UsesProcessEnvironment()
    {
        worker_threads.setEnvironmentData("TSONIC_NODE_TEST", TsValue.from("ok"));

        Assert.Equal(
            "ok",
            TsValue.CastDynamic<string>(
                worker_threads.getEnvironmentData("TSONIC_NODE_TEST")));
    }

    [Fact]
    public void MessagePort_CloseRejectsFurtherPostMessage()
    {
        var channel = new MessageChannel();
        channel.port1.close();

        Assert.Throws<System.InvalidOperationException>(() =>
            channel.port1.postMessage(TsValue.from("closed")));
        JsEventLoop.Run();
    }

    [Fact]
    public void WorkerBootstrap_RejectsMalformedCompilerOwnedArguments()
    {
        Assert.Null(worker_threads.InitializeWorkerProcess([]));
        Assert.Throws<System.InvalidOperationException>(() =>
            worker_threads.InitializeWorkerProcess(["--tsonic-node-worker-v1"]));
    }

    [Fact]
    public void TransferMarkers_PreserveExactReferenceIdentity()
    {
        var value = TsValue.from(new JSObject());

        worker_threads.markAsUntransferable(value);

        Assert.True(worker_threads.isMarkedAsUntransferable(value));
        Assert.False(worker_threads.isMarkedAsUntransferable(TsValue.from(new JSObject())));
    }
}
