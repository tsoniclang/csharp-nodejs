using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessKillTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void kill_ReturnsTrue()
    {
        using var native = IsWindows ? startWindowsSignalChild() : killTests.startSignalChild("TERM", 41);
        var child = new ChildProcess(native);
        try
        {
            if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            {
                var exception = Assert.Throws<PlatformNotSupportedException>(() => child.kill());
                Assert.Contains("ERR_FEATURE_UNAVAILABLE", exception.Message);
                Assert.False(child.killed);
                Assert.False(native.HasExited);
            }
            else
            {
                Assert.True(child.kill());
                Assert.True(child.killed);
                Assert.True(native.WaitForExit(5000));
                Assert.Equal(41, native.ExitCode);
            }
        }
        finally
        {
            killTests.reapSignalChild(native);
        }
    }

    [Fact]
    public void kill_AlreadyExited_ReturnsFalse()
    {
        var start = new ProcessStartInfo { FileName = IsWindows ? "cmd.exe" : "/bin/sh", UseShellExecute = false };
        start.ArgumentList.Add(IsWindows ? "/c" : "-c");
        start.ArgumentList.Add("exit 0");
        using var native = Process.Start(start);
        Assert.NotNull(native);
        try
        {
            Assert.True(native.WaitForExit(5000));
            var child = new ChildProcess(native);
            Assert.False(child.kill());
            Assert.False(child.killed);
        }
        finally
        {
            killTests.reapSignalChild(native);
        }
    }

    internal static Process startWindowsSignalChild()
    {
        var start = new ProcessStartInfo { FileName = "cmd.exe", UseShellExecute = false, RedirectStandardInput = true };
        start.ArgumentList.Add("/c");
        start.ArgumentList.Add("set /p value=");
        var child = Process.Start(start);
        Assert.NotNull(child);
        return child;
    }
}
