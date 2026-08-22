using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Tsonic.CSharp.Js;

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

    [Fact]
    public void spawnSyncResult_ReturnsClosedBufferCarrier()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "closed" } : new[] { "closed" };

        var result = child_process.spawnSyncResult(command, args);

        Assert.Equal(0, result.status);
        Assert.Contains("closed", result.stdout.toString("utf8"));
        Assert.Equal(string.Empty, result.stderr.toString("utf8"));
    }

    [Fact]
    public void spawnSyncResult_PreservesStartFailureWithoutThrowing()
    {
        var result = child_process.spawnSyncResult(
            "nonexistent_command_xyz",
            System.Array.Empty<string>());

        Assert.Null(result.status);
        Assert.NotEmpty(result.stderr.toString("utf8"));
    }

    [Fact]
    public void spawnSyncResult_AcceptsJavaScriptSurfaceArrayCarrier()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows
            ? new JSArray<string>(new[] { "/c", "echo", "surface" })
            : new JSArray<string>(new[] { "surface" });

        var result = child_process.spawnSyncResult(command, args);

        Assert.Equal(0, result.status);
        Assert.Contains("surface", result.stdout.toString("utf8"));
    }

    [Fact]
    public void spawnSyncResult_RejectsSparseJavaScriptArgumentArrays()
    {
        var args = JSArray<string>.fromSparse(2, (0, "present"));

        var exception = Assert.Throws<ArgumentException>(() =>
            child_process.spawnSyncResult("unused", args));

        Assert.Contains("empty element at index 1", exception.Message);
    }

    [Fact]
    public void spawnSyncResult_RejectsNullNativeArguments()
    {
        var args = new string[] { null! };

        var exception = Assert.Throws<ArgumentException>(() =>
            child_process.spawnSyncResult("unused", args));

        Assert.Contains("null at index 0", exception.Message);
    }
}
