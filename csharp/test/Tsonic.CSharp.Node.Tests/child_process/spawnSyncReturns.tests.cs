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
    public void BufferResultsUseOneGenericCarrierAndRetainOutputIdentity()
    {
        var command = IsWindows ? "cmd" : "echo";
        var arguments = IsWindows ? new[] { "/c", "echo", "generic" } : new[] { "generic" };
        var result = Assert.IsType<SpawnSyncReturns<Buffer>>(child_process.spawnSyncResult(command, arguments));

        Assert.Equal(3, result.output.Length);
        Assert.Null(result.output[0]);
        Assert.Same(result.stdout, result.output[1]);
        Assert.Same(result.stderr, result.output[2]);
        Assert.NotNull(result.stdout);
        Assert.Null(result.error);
        var missing = child_process.spawnSyncResult("nonexistent_command_xyz", Array.Empty<string>());
        Assert.All(missing.output, value => Assert.Null(value));
        Assert.Null(missing.stdout);
        Assert.Null(missing.stderr);
        Assert.NotNull(missing.error);
    }

    [Fact]
    public void UnconstrainedNativeGenericAnnotationDoesNotInventNullableValueStorage()
    {
        var result = new SpawnSyncReturns<int> { stdout = 7, stderr = 0, output = [0, 7, 0] };

        Assert.Equal(typeof(int), typeof(SpawnSyncReturns<int>).GetProperty(nameof(result.stdout))!.PropertyType);
        Assert.Equal(7, result.stdout);
        Assert.Equal(0, result.stderr);
        Assert.Equal(7, result.output[1]);
    }

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
