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
    public async System.Threading.Tasks.Task Worker_RunsBodyAndEmitsExit()
    {
        var ran = false;
        var exitCode = -1;
        var worker = new Worker(() => ran = true);
        worker.on("exit", (object? value) => exitCode = (int)value!);

        await System.Threading.Tasks.Task.Delay(100);

        Assert.True(ran);
        Assert.Equal(0, exitCode);
        Assert.True(worker.threadId > 0);
    }

    [Fact]
    public void TransferMarkers_AreClosedNoOps()
    {
        var value = new object();

        worker_threads.markAsUntransferable(value);

        Assert.False(worker_threads.isMarkedAsUntransferable(value));
    }
}
