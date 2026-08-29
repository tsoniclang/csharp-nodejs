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
    public void MessageChannel_DeliversMessagesBetweenPorts()
    {
        var channel = new MessageChannel();
        var messages = new List<string>();
        channel.port2.on<TsValue>("message", value =>
            messages.Add(TsValue.CastDynamic<string>(value)));

        channel.port1.postMessage(TsValue.from("hello"));
        JsEventLoop.Run();

        Assert.Equal(["hello"], messages);

        channel.port1.postMessage(TsValue.from("direct"));
        Assert.Equal(
            "direct",
            TsValue.CastDynamic<string>(channel.port2.receiveMessageOnPort()));
        JsEventLoop.Run();
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
