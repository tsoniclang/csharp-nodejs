using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessAllStreamPropertiesTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void allStreamProperties_AreNull()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "test" } : new[] { "test" };
        var child = child_process.spawn(command, args);

        // All stream properties should be null (streams not yet implemented)
        Assert.Null(child.stdin);
        Assert.Null(child.stdout);
        Assert.Null(child.stderr);
    }
}
