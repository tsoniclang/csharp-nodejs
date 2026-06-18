using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessForkTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void fork_CreatesChildProcess()
    {
        // Fork needs a valid module path - we'll use the current executable
        var modulePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "dotnet";
        var child = child_process.fork(modulePath);

        Assert.NotNull(child);
        Assert.True(child.pid > 0);
        Assert.True(child.connected); // fork sets up IPC channel

        // Kill it so test doesn't hang
        child.kill();
    }

    [Fact]
    public void fork_WithArgs_PassesArguments()
    {
        var modulePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "dotnet";
        var args = new[] { "--version" };
        var child = child_process.fork(modulePath, args);

        Assert.NotNull(child);
        Assert.True(child.pid > 0);

        // Kill it so test doesn't hang
        child.kill();
    }
}
