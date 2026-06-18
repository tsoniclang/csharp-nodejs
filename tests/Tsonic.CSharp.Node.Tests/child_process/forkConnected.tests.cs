using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessForkConnectedTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void forkConnected_InitiallyTrue()
    {
        var modulePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "dotnet";
        var child = child_process.fork(modulePath);

        Assert.True(child.connected);

        child.kill();
    }
}
