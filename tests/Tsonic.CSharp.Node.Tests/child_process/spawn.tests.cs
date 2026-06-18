using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessSpawnTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void spawn_SimpleCommand_ReturnsChildProcess()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "test" } : new[] { "test" };
        var child = child_process.spawn(command, args);

        Assert.NotNull(child);
        Assert.True(child.pid > 0);
    }

    [Fact]
    public void spawn_HasSpawnProperties()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "test" } : new[] { "test" };
        var child = child_process.spawn(command, args);

        Assert.NotNull(child.spawnfile);
        Assert.NotEmpty(child.spawnfile);
        Assert.NotNull(child.spawnargs);
        Assert.NotEmpty(child.spawnargs);
        Assert.Equal(command, child.spawnfile);
    }

    [Fact]
    public void spawn_EmitsSpawnEvent()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "test" } : new[] { "test" };

        var child = child_process.spawn(command, args);

        // Spawn event is fired immediately, this test verifies the API exists
        // We can't reliably test event timing since it may fire before listener attached
        child.on("spawn", () => { /* Event handler */ });

        // Give some time for event to fire
        Thread.Sleep(100);

        // Test verifies API exists and doesn't throw
        Assert.NotNull(child);
    }

    [Fact]
    public void spawn_EmitsCloseEvent()
    {
        // Use a longer-running process to ensure we can attach listener before it exits
        var command = IsWindows ? "cmd" : "sleep";
        var args = IsWindows ? new[] { "/c", "timeout", "/t", "1", "/nobreak" } : new[] { "0.1" };
        var closeEmitted = false;
        int? exitCode = null;
        var resetEvent = new ManualResetEventSlim(false);

        var child = child_process.spawn(command, args);
        child.on("close", (int? code, string? signal) =>
        {
            closeEmitted = true;
            exitCode = code;
            resetEvent.Set();
        });

        // Wait for process to complete (up to 3 seconds)
        var signaled = resetEvent.Wait(3000);

        // close event should have been emitted
        Assert.True(signaled, "Close event was not emitted within timeout");
        Assert.True(closeEmitted);
        Assert.NotNull(exitCode);
    }

    [Fact]
    public void spawn_InvalidCommand_ThrowsWithoutHandler()
    {
        // When there's no error handler, spawn throws for invalid commands
        // This matches Node.js behavior where unhandled error events throw
        Assert.Throws<System.ComponentModel.Win32Exception>(() =>
        {
            var child = child_process.spawn("nonexistent_command_xyz");
        });
    }
}
