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
        using var native = IsWindows ? ChildProcessKillTests.startWindowsSignalChild() : killTests.startSignalChild("TERM", 41);
        var child = new ChildProcess(native);
        try
        {
            if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            {
                var exception = Assert.Throws<PlatformNotSupportedException>(() => child.kill("SIGKILL"));
                Assert.Contains("ERR_FEATURE_UNAVAILABLE", exception.Message);
                Assert.False(child.killed);
                Assert.False(native.HasExited);
            }
            else
            {
                Assert.True(child.kill("SIGKILL"));
                Assert.True(child.killed);
                Assert.True(native.WaitForExit(5000));
            }
        }
        finally
        {
            killTests.reapSignalChild(native);
        }
    }

    [Fact]
    public void killWithSignal_WithDifferentSignals_Works()
    {
        if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            foreach (var signal in new string?[] { "SIGTERM", "SIGKILL", null })
            {
                using var native = killTests.startSignalChild("TERM", 41);
                var child = new ChildProcess(native);
                try
                {
                    Assert.True(child.kill(signal));
                    Assert.True(child.killed);
                    Assert.True(native.WaitForExit(5000));
                    if (signal != "SIGKILL")
                        Assert.Equal(41, native.ExitCode);
                }
                finally
                {
                    killTests.reapSignalChild(native);
                }
            }
        }
    }

    [Fact]
    public void killWithSignal_ShouldShareNativeDeliveryAndUnknownSignalRefusal()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            return;
        using var native = killTests.startSignalChild("USR1", 37);
        var child = new ChildProcess(native);
        try
        {
            Assert.Contains("ERR_UNKNOWN_SIGNAL", Assert.Throws<ArgumentException>(() => child.kill("BOGUS")).Message);
            Assert.False(child.killed);
            Assert.True(process.kill(native.Id, 0));
            Assert.True(child.kill("SIGUSR1"));
            Assert.True(child.killed);
            Assert.True(native.WaitForExit(5000));
            Assert.Equal(37, native.ExitCode);
        }
        finally
        {
            killTests.reapSignalChild(native);
        }
    }
}
