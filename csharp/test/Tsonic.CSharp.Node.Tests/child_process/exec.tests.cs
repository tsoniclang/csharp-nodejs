using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessExecTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void exec_CallsCallback()
    {
        var command = IsWindows ? "echo Hello" : "echo 'Hello'";
        var resetEvent = new ManualResetEventSlim(false);
        var callbackCalled = false;
        var stdout = "";

        child_process.exec(command, (error, stdoutStr, stderr) =>
        {
            callbackCalled = true;
            stdout = stdoutStr;
            resetEvent.Set();
        });

        var signaled = resetEvent.Wait(5000);
        Assert.True(signaled, "exec callback was not called within timeout");
        Assert.True(callbackCalled);
        Assert.Contains("Hello", stdout);
    }

    [Fact]
    public void exec_WithOptions_CallsCallback()
    {
        var command = IsWindows ? "echo Test" : "echo 'Test'";
        var options = new ExecOptions { encoding = "utf8" };
        var resetEvent = new ManualResetEventSlim(false);
        var callbackCalled = false;

        child_process.exec(command, options, (error, stdout, stderr) =>
        {
            callbackCalled = true;
            resetEvent.Set();
        });

        var signaled = resetEvent.Wait(5000);
        Assert.True(signaled, "exec callback was not called within timeout");
        Assert.True(callbackCalled);
    }

    [Fact]
    public void exec_FailingCommand_PassesErrorToCallback()
    {
        var command = IsWindows ? "exit /b 1" : "exit 1";
        var resetEvent = new ManualResetEventSlim(false);
        Exception? capturedError = null;

        child_process.exec(command, (error, stdout, stderr) =>
        {
            capturedError = error;
            resetEvent.Set();
        });

        var signaled = resetEvent.Wait(5000);
        Assert.True(signaled, "exec error callback was not called within timeout");
        Assert.NotNull(capturedError);
    }
}
