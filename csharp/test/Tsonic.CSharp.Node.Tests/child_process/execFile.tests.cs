using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessExecFileTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void execFile_CallsCallback()
    {
        var file = IsWindows ? "cmd.exe" : "/bin/echo";
        var args = IsWindows ? new[] { "/c", "echo", "Hello" } : new[] { "Hello" };
        var resetEvent = new ManualResetEventSlim(false);
        var callbackCalled = false;
        var stdout = "";

        child_process.execFile(file, args, null, (error, stdoutStr, stderr) =>
        {
            callbackCalled = true;
            stdout = stdoutStr;
            resetEvent.Set();
        });

        var signaled = resetEvent.Wait(5000);
        Assert.True(signaled, "execFile callback was not called within timeout");
        Assert.True(callbackCalled);
        Assert.Contains("Hello", stdout);
    }
}
