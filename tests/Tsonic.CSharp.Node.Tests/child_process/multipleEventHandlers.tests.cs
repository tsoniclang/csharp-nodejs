using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessMultipleEventHandlersTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void multipleEventHandlers_AllCalled()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "test" } : new[] { "test" };
        var handler1Called = false;
        var handler2Called = false;
        var resetEvent = new ManualResetEventSlim(false);
        var callCount = 0;

        var child = child_process.spawn(command, args);

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

        resetEvent.Wait(2000);

        Assert.True(handler1Called);
        Assert.True(handler2Called);
    }
}
