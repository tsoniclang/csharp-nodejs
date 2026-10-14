using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Tsonic.CSharp.Js;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public sealed class MessagePortOwnerTests
{
    [Fact]
    public void NativeLocalTransportWorksWithoutAnEventLoop()
    {
        var channel = new MessageChannel();
        using var sender = channel.port1;
        using var receiver = channel.port2;
        Assert.Throws<InvalidOperationException>(() => JsEventLoop.Capture());
        sender.postMessage(TsValue.from(9_007_199_254_740_993L));
        Assert.Equal(9_007_199_254_740_993L, Assert.IsType<long>(receiver.receiveMessageOnPort().unwrap()));
        sender.postMessage(TsValue.from((object?)null));
        Assert.Null(receiver.receiveMessageOnPort().unwrap());
        Assert.Null(receiver.receiveMessageOnPort().unwrap());
        Assert.Same(receiver, receiver.unref());
        Assert.False(receiver.hasRef());
        Assert.Same(receiver, receiver.@ref());
        Assert.True(receiver.hasRef());
        Assert.Throws<InvalidOperationException>(() => JsEventLoop.Capture());
    }

    [Fact]
    public void CallbackRegistrationRequiresAnOwnerBeforeAnyListenerSideEffect()
    {
        var channel = new MessageChannel();
        using var sender = channel.port1;
        using var receiver = channel.port2;
        var called = false;
        var failure = Assert.Throws<InvalidOperationException>(() =>
            receiver.on<TsValue>("message", _ => called = true));
        Assert.Equal("No JavaScript event loop is running on this thread.", failure.Message);
        Assert.Equal(0, receiver.listenerCount("message"));
        Assert.Throws<InvalidOperationException>(receiver.start);
        sender.postMessage(TsValue.from(31));
        JsEventLoop.Run();
        Assert.False(called);
        Assert.Equal(31, Assert.IsType<int>(receiver.receiveMessageOnPort().unwrap()));
    }

    [Fact]
    public void AdmissionFailureDoesNotStoreListenersOrEmitNewListener()
    {
        var emitter = new AdmissionEmitter();
        var metaCalls = 0;
        emitter.on("newListener", () => metaCalls++);
        emitter.Reject = true;
        var original = Assert.Throws<InvalidOperationException>(() => emitter.on("message", static () => { }));
        Assert.Same(emitter.Failure, original);
        Assert.Equal(0, emitter.listenerCount("message"));
        Assert.Equal(0, metaCalls);
    }

    [Fact]
    public void QueuedNativeMessagesAttachToAnExplicitLoopAndKeepTheirExactCarrier()
    {
        var channel = new MessageChannel();
        using var sender = channel.port1;
        using var receiver = channel.port2;
        sender.postMessage(TsValue.from(ulong.MaxValue));
        var values = new List<ulong>();
        var owner = Environment.CurrentManagedThreadId;
        JsEventLoop.Run(() =>
        {
            receiver.on<TsValue>("message", value =>
            {
                Assert.Equal(owner, Environment.CurrentManagedThreadId);
                values.Add(Assert.IsType<ulong>(value.unwrap()));
            });
            Assert.Empty(values);
        });
        Assert.Equal(new[] { ulong.MaxValue }, values);
        Assert.Equal(0, receiver.listenerCount("message"));
    }

    [Fact]
    public void RetirementReleasesCapturesWithoutClosingThePhysicalLocalChannel()
    {
        var channel = new MessageChannel();
        using var sender = channel.port1;
        using var receiver = channel.port2;
        var calls = new List<int>();
        var capture = InstallRetiredCapture(receiver, calls);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Assert.False(capture.IsAlive);
        Assert.Empty(calls);
        Assert.Equal(0, receiver.listenerCount("message"));
        Assert.Equal(0, receiver.listenerCount("removeListener"));
        sender.postMessage(TsValue.from(9_007_199_254_740_993L));
        Assert.Equal(9_007_199_254_740_993L, Assert.IsType<long>(receiver.receiveMessageOnPort().unwrap()));
        JsEventLoop.Run();
        Assert.Empty(calls);
    }

    [Fact]
    public void RetiredQueuedCallbacksCannotConsumeNextGenerationMessages()
    {
        var channel = new MessageChannel();
        using var sender = channel.port1;
        using var receiver = channel.port2;
        var oldCalls = 0;
        var original = new InvalidOperationException("stop original generation");
        var actual = Assert.Throws<InvalidOperationException>(() => JsEventLoop.Run(() =>
        {
            receiver.on<TsValue>("message", _ => oldCalls++);
            sender.postMessage(TsValue.from(11));
            JsEventLoop.Enqueue(() => throw original);
        }));
        Assert.Same(original, actual);
        Assert.Equal(0, oldCalls);
        Assert.Equal(0, receiver.listenerCount("message"));
        sender.postMessage(TsValue.from(12));
        var fresh = new List<int>();
        JsEventLoop.Run(() => receiver.on<TsValue>("message", value =>
            fresh.Add(Assert.IsType<int>(value.unwrap()))));
        Assert.Equal(new[] { 11, 12 }, fresh);
        Assert.Equal(0, oldCalls);
    }

    [Fact]
    public void BoundedNativeQueueRejectsAtomicallyAndRecoversAfterOneRead()
    {
        var channel = new MessageChannel();
        using var sender = channel.port1;
        using var receiver = channel.port2;
        for (var index = 0; index < 65536; index++)
            sender.postMessage(TsValue.from(index));
        var failure = Assert.Throws<InvalidOperationException>(() => sender.postMessage(TsValue.from(-1)));
        Assert.Equal("MessagePort queue exceeds the finite message limit.", failure.Message);
        Assert.Equal(0, Assert.IsType<int>(receiver.receiveMessageOnPort().unwrap()));
        sender.postMessage(TsValue.from(65536));
        for (var index = 1; index <= 65536; index++)
            Assert.Equal(index, Assert.IsType<int>(receiver.receiveMessageOnPort().unwrap()));
        Assert.Null(receiver.receiveMessageOnPort().unwrap());
    }

    [Fact]
    public void ClosingAnEndpointRejectsItsPeerWithoutMutatingTheQueue()
    {
        var channel = new MessageChannel();
        using var sender = channel.port1;
        using var receiver = channel.port2;
        receiver.close();
        Assert.Throws<InvalidOperationException>(() => sender.postMessage(TsValue.from(7)));
        Assert.Null(receiver.receiveMessageOnPort().unwrap());
        Assert.False(receiver.hasRef());
        receiver.@ref();
        Assert.False(receiver.hasRef());
    }

    [Fact]
    public void PhysicalTcpTransportCanReceiveAndPostBeforeAndAfterLoopRetirement()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        using var peerClient = new TcpClient(AddressFamily.InterNetwork);
        peerClient.ReceiveTimeout = 5000;
        peerClient.Connect((IPEndPoint)listener.LocalEndpoint);
        using var transport = new WorkerTransport(listener.AcceptTcpClient());
        using var peer = new WorkerTransport(peerClient);
        using var port = new MessagePort(transport);
        port.StartTransportReader();
        peer.Send(WorkerFrameKind.Message, StructuredClone.Encode(TsValue.from(71L)));
        Assert.Equal(71L, Assert.IsType<long>(ReceiveEventually(port).unwrap()));
        port.postMessage(TsValue.from(72UL));
        var frame = peer.Receive();
        Assert.Equal(WorkerFrameKind.Message, frame.Kind);
        Assert.Equal(72UL, Assert.IsType<ulong>(StructuredClone.Decode(frame.Payload).unwrap()));
        var seen = 0;
        JsEventLoop.Run(() =>
        {
            port.on<TsValue>("message", value =>
            {
                seen = Assert.IsType<int>(value.unwrap());
                port.unref();
            });
            Assert.True(ProcessKeepAlive.HasReferences);
            peer.Send(WorkerFrameKind.Message, StructuredClone.Encode(TsValue.from(73)));
        });
        Assert.Equal(73, seen);
        Assert.Equal(0, port.listenerCount("message"));
        port.@ref();
        Assert.True(port.hasRef());
        Assert.False(ProcessKeepAlive.HasReferences);
        peer.Send(WorkerFrameKind.Message, StructuredClone.Encode(TsValue.from(74)));
        Assert.Equal(74, Assert.IsType<int>(ReceiveEventually(port).unwrap()));
        port.postMessage(TsValue.from(75));
        Assert.Equal(75, Assert.IsType<int>(StructuredClone.Decode(peer.Receive().Payload).unwrap()));
    }

    [Fact]
    public void NativeForeignMessagesRunOnlyOnTheirCapturedCallbackOwner()
    {
        var channel = new MessageChannel();
        using var sender = channel.port1;
        using var receiver = channel.port2;
        var owner = Environment.CurrentManagedThreadId;
        var seen = 0;
        JsEventLoop.Run(async () =>
        {
            receiver.on<TsValue>("message", value =>
            {
                Assert.Equal(owner, Environment.CurrentManagedThreadId);
                seen = Assert.IsType<int>(value.unwrap());
            });
            await Task.Run(() => sender.postMessage(TsValue.from(81)));
        });
        Assert.Equal(81, seen);
    }

    [Fact]
    public void NativeSignalAdmissionPreservesFifoAcrossPhysicalPorts()
    {
        var first = new MessageChannel();
        var second = new MessageChannel();
        using var firstSender = first.port1;
        using var firstReceiver = first.port2;
        using var secondSender = second.port1;
        using var secondReceiver = second.port2;
        var seen = new List<int>();
        JsEventLoop.Run(() =>
        {
            firstReceiver.on<TsValue>("message", value => seen.Add(Assert.IsType<int>(value.unwrap())));
            secondReceiver.on<TsValue>("message", value => seen.Add(Assert.IsType<int>(value.unwrap())));
            firstSender.postMessage(TsValue.from(1));
            firstSender.postMessage(TsValue.from(2));
            secondSender.postMessage(TsValue.from(3));
        });
        Assert.Equal(new[] { 1, 2, 3 }, seen);
    }

    [Fact]
    public void ForeignLoopCannotAdoptAnActiveOwnersCallbacks()
    {
        var channel = new MessageChannel();
        using var sender = channel.port1;
        using var receiver = channel.port2;
        var metaCalls = 0;
        var firstCalls = 0;
        var foreignCalls = 0;
        JsEventLoop.Run(async () =>
        {
            receiver.on("newListener", () => metaCalls++);
            receiver.on<TsValue>("message", _ => firstCalls++);
            Assert.Equal(1, metaCalls);
            await Task.Run(() => JsEventLoop.Run(() =>
            {
                var failure = Assert.Throws<InvalidOperationException>(() =>
                    receiver.on<TsValue>("message", _ => foreignCalls++));
                Assert.Equal("MessagePort callbacks belong to another active event loop.", failure.Message);
                Assert.Equal(1, receiver.listenerCount("message"));
                Assert.Equal(1, metaCalls);
            }));
            sender.postMessage(TsValue.from(9));
        });
        Assert.Equal(1, firstCalls);
        Assert.Equal(0, foreignCalls);
    }

    [Fact]
    public void RegistrationCapacityFailureLeavesTheNativeMessageAndListenerStoreUntouched()
    {
        var channel = new MessageChannel();
        using var sender = channel.port1;
        using var receiver = channel.port2;
        sender.postMessage(TsValue.from(ulong.MaxValue));
        JsEventLoop.Run(() =>
        {
            var dispatch = JsEventLoop.Capture();
            for (var index = 0; index < 16384; index++) dispatch.Register(static () => { });
            Assert.Throws<InvalidOperationException>(() => receiver.on("message", static () => { }));
            Assert.Equal(0, receiver.listenerCount("message"));
            Assert.Equal(ulong.MaxValue, Assert.IsType<ulong>(receiver.receiveMessageOnPort().unwrap()));
            Assert.False(ProcessKeepAlive.HasReferences);
        });
    }

    [Fact]
    public void NativeProtocolFailureWaitsForExplicitCallbacksAndClosesOnlyItsTransport()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        using var peerClient = new TcpClient(AddressFamily.InterNetwork);
        peerClient.ReceiveTimeout = 5000;
        peerClient.Connect((IPEndPoint)listener.LocalEndpoint);
        using var transport = new WorkerTransport(listener.AcceptTcpClient());
        using var peer = new WorkerTransport(peerClient);
        using var port = new MessagePort(transport);
        port.StartTransportReader();
        peer.Send((WorkerFrameKind)99, []);
        Assert.True(SpinWait.SpinUntil(() => !port.hasRef(), TimeSpan.FromSeconds(5)), "native protocol failure deadline");
        var errors = new List<Exception>();
        var closed = 0;
        var removed = 0;
        JsEventLoop.Run(() =>
        {
            port.on<Exception>("error", error => errors.Add(error));
            port.on("close", () => closed++);
            port.on("removeListener", () => removed++);
            Assert.Empty(errors);
        });
        Assert.IsType<InvalidDataException>(Assert.Single(errors));
        Assert.Equal("Unexpected worker message-port frame '99'.", errors[0].Message);
        Assert.Equal(1, closed);
        Assert.Equal(0, removed);
        Assert.Equal(0, port.listenerCount("error"));
        Assert.Equal(WorkerFrameKind.Close, peer.Receive().Kind);
    }

    [Fact]
    public void NativeErrorMessageAndCloseJobsPreserveTheirAdmissionOrder()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        using var peerClient = new TcpClient(AddressFamily.InterNetwork);
        peerClient.ReceiveTimeout = 5000;
        peerClient.Connect((IPEndPoint)listener.LocalEndpoint);
        using var transport = new WorkerTransport(listener.AcceptTcpClient());
        using var peer = new WorkerTransport(peerClient);
        using var port = new MessagePort(transport);
        var seen = new List<string>();
        JsEventLoop.Run(() =>
        {
            port.on<Exception>("error", error =>
            {
                Assert.Equal("native error", error.Message);
                seen.Add("error");
            });
            port.on<TsValue>("message", value =>
            {
                Assert.Equal(83, Assert.IsType<int>(value.unwrap()));
                seen.Add("message");
            });
            port.on("close", () => seen.Add("close"));
            peer.Send(WorkerFrameKind.Error, System.Text.Encoding.UTF8.GetBytes("native error"));
            peer.Send(WorkerFrameKind.Message, StructuredClone.Encode(TsValue.from(83)));
            peer.Send(WorkerFrameKind.Close, []);
            Assert.True(SpinWait.SpinUntil(() => !port.hasRef(), TimeSpan.FromSeconds(5)), "native ordered frames deadline");
            Assert.Empty(seen);
        });
        Assert.Equal(new[] { "error", "message", "close" }, seen);
        Assert.Equal(WorkerFrameKind.Close, peer.Receive().Kind);
    }

    [Fact]
    public void ForeignNativeCloseIsAdmittedBeforeTheLastReferenceIsReleased()
    {
        var closed = 0;
        for (var iteration = 0; iteration < 64; iteration++)
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            using var peerClient = new TcpClient(AddressFamily.InterNetwork);
            peerClient.ReceiveTimeout = 5000;
            peerClient.Connect((IPEndPoint)listener.LocalEndpoint);
            using var transport = new WorkerTransport(listener.AcceptTcpClient());
            using var peer = new WorkerTransport(peerClient);
            using var port = new MessagePort(transport);
            JsEventLoop.Run(() =>
            {
                port.on("close", () => closed++);
                Assert.True(ProcessKeepAlive.HasReferences);
                peer.Send(WorkerFrameKind.Close, []);
            });
            Assert.Equal(iteration + 1, closed);
            Assert.Equal(WorkerFrameKind.Close, peer.Receive().Kind);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference InstallRetiredCapture(MessagePort port, List<int> calls)
    {
        var captured = new object();
        var weak = new WeakReference(captured);
        JsEventLoop.Run(() =>
        {
            port.on<TsValue>("message", _ =>
            {
                GC.KeepAlive(captured);
                calls.Add(1);
            });
            port.on("removeListener", () => calls.Add(2));
        });
        return weak;
    }

    private static TsValue ReceiveEventually(MessagePort port)
    {
        var selected = TsValue.undefined();
        Assert.True(SpinWait.SpinUntil(() =>
        {
            selected = port.receiveMessageOnPort();
            return selected.unwrap() is not null;
        }, TimeSpan.FromSeconds(5)), "native message receive deadline");
        return selected;
    }

    private sealed class AdmissionEmitter : EventEmitter
    {
        internal readonly InvalidOperationException Failure = new("rejected before registration");
        internal bool Reject;
        protected override void OnListenerAdding(object eventName)
        {
            if (Reject) throw Failure;
        }
    }
}
