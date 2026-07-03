using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessDisconnectEventTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void disconnectEvent_CanBeEmitted()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "test" } : new[] { "test" };

        var child = child_process.spawn(command, args);
        child.on("disconnect", () =>
        {
            // Disconnect event handler attached
        });

        child.disconnect();

        // disconnect() immediately sets connected to false
        Assert.False(child.connected);
    }
}
