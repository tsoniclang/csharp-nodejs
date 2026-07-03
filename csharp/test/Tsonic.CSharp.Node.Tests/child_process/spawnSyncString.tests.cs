using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessSpawnSyncStringTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void spawnSyncString_ReturnsStringOutput()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "Hello" } : new[] { "Hello" };
        var result = child_process.spawnSyncString(command, args);

        Assert.NotNull(result);
        Assert.IsType<string>(result.stdout);
        Assert.Contains("Hello", result.stdout);
    }

    [Fact]
    public void spawnSyncString_WithOptions_ReturnsStringOutput()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "Test" } : new[] { "Test" };
        var options = new ExecOptions { encoding = "utf8" };
        var result = child_process.spawnSyncString(command, args, options);

        Assert.IsType<string>(result.stdout);
        Assert.IsType<string>(result.stderr);
    }
}
