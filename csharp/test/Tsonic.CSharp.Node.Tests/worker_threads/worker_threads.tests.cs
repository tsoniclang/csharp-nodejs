using System.Collections.Generic;
using Tsonic.CSharp.Node;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class WorkerThreadsTests
{
    [Fact]
    public void MessageChannel_DeliversMessagesBetweenPorts()
    {
        var channel = new MessageChannel();
        var messages = new List<object?>();
        channel.port2.on("message", (object? value) => messages.Add(value));

        channel.port1.postMessage("hello");

        Assert.Equal(["hello"], messages);
        Assert.Equal("hello", channel.port2.receiveMessageOnPort());
    }

    [Fact]
    public void EnvironmentData_UsesProcessEnvironment()
    {
        worker_threads.setEnvironmentData("TSONIC_NODE_TEST", "ok");

        Assert.Equal("ok", worker_threads.getEnvironmentData("TSONIC_NODE_TEST"));
    }

    [Fact]
    public void MessagePort_CloseRejectsFurtherPostMessage()
    {
        var channel = new MessageChannel();
        channel.port1.close();

        Assert.Throws<System.InvalidOperationException>(() => channel.port1.postMessage("closed"));
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
        var value = new object();

        worker_threads.markAsUntransferable(value);

        Assert.True(worker_threads.isMarkedAsUntransferable(value));
        Assert.False(worker_threads.isMarkedAsUntransferable(new object()));
    }
}
