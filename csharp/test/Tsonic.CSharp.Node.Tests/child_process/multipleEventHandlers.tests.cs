using Xunit;
using System.Threading;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessMultipleEventHandlersTests
{
    [Fact]
    public void multipleEventHandlers_AllCalled()
    {
        var handler1Called = false;
        var handler2Called = false;
        var resetEvent = new ManualResetEventSlim(false);
        var callCount = 0;

        using var exitGate = ChildProcessExitGate.Spawn(0);
        var child = exitGate.Child;

        child.on("exit", (int? code, string? signal) =>
        {
            handler1Called = true;
            if (++callCount == 2) resetEvent.Set();
        });

        child.on("exit", (int? code, string? signal) =>
        {
            handler2Called = true;
            if (++callCount == 2) resetEvent.Set();
        });

        exitGate.Release();
        var signaled = resetEvent.Wait(5000);

        Assert.True(signaled, "exit event handlers were not invoked within timeout");
        Assert.True(handler1Called);
        Assert.True(handler2Called);
    }
}
