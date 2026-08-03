using Xunit;
using System;
using System.Threading;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessExitTests
{
    [Fact]
    public void exit_ExitEvent_ContainsExitCode()
    {
        int? capturedExitCode = null;
        var resetEvent = new ManualResetEventSlim(false);

        using var exitGate = ChildProcessExitGate.Spawn(0);
        var child = exitGate.Child;
        child.on("exit", (int? code, string? signal) =>
        {
            capturedExitCode = code;
            resetEvent.Set();
        });

        exitGate.Release();
        var signaled = resetEvent.Wait(5000);

        Assert.True(signaled, "exit event was not emitted within timeout");
        Assert.NotNull(capturedExitCode);
        Assert.Equal(0, capturedExitCode.Value);
    }

    [Fact]
    public void exit_ExitEvent_NonZeroExitCode()
    {
        int? capturedExitCode = null;
        var resetEvent = new ManualResetEventSlim(false);

        using var exitGate = ChildProcessExitGate.Spawn(42);
        var child = exitGate.Child;
        child.on("exit", (int? code, string? signal) =>
        {
            capturedExitCode = code;
            resetEvent.Set();
        });

        exitGate.Release();
        var signaled = resetEvent.Wait(5000);

        Assert.True(signaled, "exit event was not emitted within timeout");
        Assert.Equal(42, capturedExitCode);
    }

    [Fact]
    public void exit_ExitCodeProperty_SetAfterExit()
    {
        var resetEvent = new ManualResetEventSlim(false);

        using var exitGate = ChildProcessExitGate.Spawn(0);
        var child = exitGate.Child;
        child.on("exit", (int? code, string? signal) =>
        {
            resetEvent.Set();
        });

        exitGate.Release();
        var signaled = resetEvent.Wait(5000);
        Assert.True(signaled, "exit event was not emitted within timeout");
        SpinWait.SpinUntil(() => child.exitCode != null, TimeSpan.FromSeconds(1));

        Assert.NotNull(child.exitCode);
    }
}
