using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessMessageTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void message_MessageEvent_CanBeAttached()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "test" } : new[] { "test" };

        var child = child_process.spawn(command, args);

        // Verify we can attach message event handler
        child.on("message", (object message, object sendHandle) =>
        {
            // Handler attached successfully
        });

        Assert.NotNull(child);
    }
}
