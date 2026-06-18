using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessSpawnSyncTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void spawnSync_SimpleCommand_ReturnsResult()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "Hello" } : new[] { "Hello" };
        var result = child_process.spawnSync(command, args);

        Assert.NotNull(result);
        Assert.True(result.status == 0 || result.status == null);
        Assert.NotNull(result.stdout);
    }

    [Fact]
    public void spawnSync_HasPid()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "test" } : new[] { "test" };
        var result = child_process.spawnSync(command, args);

        Assert.True(result.pid > 0);
    }

    [Fact]
    public void spawnSync_WithInvalidCommand_SetsError()
    {
        var result = child_process.spawnSync("nonexistent_command_xyz");

        Assert.NotNull(result.error);
    }

    [Fact]
    public void spawnSync_OutputArray_ContainsStdoutStderr()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "test" } : new[] { "test" };
        var result = child_process.spawnSync(command, args);

        Assert.NotNull(result.output);
        Assert.True(result.output.Length >= 3);
        // output[0] is null (stdin), output[1] is stdout, output[2] is stderr
    }
}
