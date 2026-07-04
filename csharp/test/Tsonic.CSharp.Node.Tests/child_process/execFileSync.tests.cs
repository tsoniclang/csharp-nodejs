using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessExecFileSyncTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void execFileSync_SimpleCommand_ReturnsOutput()
    {
        var file = IsWindows ? "cmd.exe" : "/bin/echo";
        var args = IsWindows ? new[] { "/c", "echo", "Hello" } : new[] { "Hello" };
        var result = child_process.execFileSync(file, args);

        Assert.NotNull(result);
    }

    [Fact]
    public void execFileSync_WithStringEncoding_ReturnsString()
    {
        var file = IsWindows ? "cmd.exe" : "/bin/echo";
        var args = IsWindows ? new[] { "/c", "echo", "Hello" } : new[] { "Hello" };
        var options = new ExecOptions { encoding = "utf8" };
        var result = child_process.execFileSync(file, args, options);

        Assert.IsType<string>(result);
        Assert.Contains("Hello", (string)result);
    }
}
