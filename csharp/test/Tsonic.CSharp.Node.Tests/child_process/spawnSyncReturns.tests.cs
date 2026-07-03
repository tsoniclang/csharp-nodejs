using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessSpawnSyncReturnsTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void spawnSyncReturns_AllPropertiesAccessible()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "test" } : new[] { "test" };
        var result = child_process.spawnSync(command, args);

        // Verify all properties are accessible
        Assert.True(result.pid > 0);
        Assert.NotNull(result.output);
        Assert.NotNull(result.stdout);
        Assert.NotNull(result.stderr);
        Assert.True(result.status == 0 || result.status == null);
        // signal is null for successful execution
        // error is null for successful execution
    }
}
