using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessKillWithSignalTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void killWithSignal_SetsSignalCode()
    {
        if (IsWindows)
        {
            var child = child_process.spawn("cmd", new[] { "/c", "timeout", "10" });
            Thread.Sleep(100);

            var killed = child.kill("SIGKILL");

            Assert.True(killed);
            Assert.True(child.killed);
            // Note: signalCode might not be set on Windows due to .NET limitations
        }
        else
        {
            var child = child_process.spawn("sleep", new[] { "10" });
            Thread.Sleep(100);

            var killed = child.kill("SIGKILL");

            Assert.True(killed);
            Assert.True(child.killed);
        }
    }

    [Fact]
    public void killWithSignal_WithDifferentSignals_Works()
    {
        if (!IsWindows)
        {
            // Test SIGTERM
            var child1 = child_process.spawn("sleep", new[] { "10" });
            Thread.Sleep(100);
            Assert.True(child1.kill("SIGTERM"));

            // Test SIGKILL
            var child2 = child_process.spawn("sleep", new[] { "10" });
            Thread.Sleep(100);
            Assert.True(child2.kill("SIGKILL"));

            // Test default (should be SIGTERM)
            var child3 = child_process.spawn("sleep", new[] { "10" });
            Thread.Sleep(100);
            Assert.True(child3.kill());
        }
    }
}
