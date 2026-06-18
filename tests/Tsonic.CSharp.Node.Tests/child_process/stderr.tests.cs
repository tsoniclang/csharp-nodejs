using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessStderrTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void stderr_StderrProperty_IsAccessible()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "test" } : new[] { "test" };
        var child = child_process.spawn(command, args);

        // stderr is null in current implementation (streams not yet implemented)
        // but property should be accessible
        var stderr = child.stderr;
        Assert.Null(stderr);
    }
}
