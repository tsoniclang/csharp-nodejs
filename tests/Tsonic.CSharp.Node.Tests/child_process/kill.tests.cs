using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessKillTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void kill_ReturnsTrue()
    {
        if (IsWindows)
        {
            // On Windows, use a command that will run for a bit
            var child = child_process.spawn("cmd", new[] { "/c", "timeout", "5" });
            Thread.Sleep(100); // Let it start

            var killed = child.kill();
            Assert.True(killed);
            Assert.True(child.killed);
        }
        else
        {
            var child = child_process.spawn("sleep", new[] { "5" });
            Thread.Sleep(100); // Let it start

            var killed = child.kill();
            Assert.True(killed);
            Assert.True(child.killed);
        }
    }

    [Fact]
    public void kill_AlreadyExited_ReturnsFalse()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "test" } : new[] { "test" };
        var child = child_process.spawn(command, args);

        // Wait for process to complete
        Thread.Sleep(500);

        // Try to kill already-exited process
        var killed = child.kill();
        Assert.False(killed);
    }
}
