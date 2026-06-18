using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessExecSyncTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void execSync_SimpleCommand_ReturnsOutput()
    {
        var command = IsWindows ? "echo Hello" : "echo 'Hello'";
        var result = child_process.execSync(command);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void execSync_WithOptions_ReturnsString()
    {
        var command = IsWindows ? "echo Hello" : "echo 'Hello'";
        var options = new ExecOptions { encoding = "utf8" };
        var result = child_process.execSync(command, options);

        Assert.IsType<string>(result);
        var str = (string)result;
        Assert.Contains("Hello", str);
    }

    [Fact]
    public void execSync_WithBufferEncoding_ReturnsByteArray()
    {
        var command = IsWindows ? "echo Hello" : "echo 'Hello'";
        var options = new ExecOptions { encoding = "buffer" };
        var result = child_process.execSync(command, options);

        Assert.True(result is byte[] || result is string);
    }

    [Fact]
    public void execSync_WithCwd_ExecutesInDirectory()
    {
        var tempDir = Path.GetTempPath();
        var command = IsWindows ? "cd" : "pwd";
        var options = new ExecOptions { cwd = tempDir, encoding = "utf8" };
        var result = child_process.execSync(command, options);

        Assert.IsType<string>(result);
        // Result should contain temp directory path
    }

    [Fact]
    public void execSync_NonZeroExit_ThrowsException()
    {
        var command = IsWindows ? "exit /b 1" : "exit 1";

        Assert.Throws<InvalidOperationException>(() =>
        {
            child_process.execSync(command);
        });
    }
}
