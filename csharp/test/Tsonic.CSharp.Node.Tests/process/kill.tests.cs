using Xunit;
using System.Diagnostics;
using System.ComponentModel;

namespace Tsonic.CSharp.Node.Tests;

public class killTests
{
    [Fact]
    public void kill_ShouldThrowForNonExistentProcess()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            return;
        var nonExistentPid = int.MaxValue;
        Assert.Throws<ArgumentException>(() =>
        {
            using var unexpectedProcess = Process.GetProcessById(nonExistentPid);
        });

        var exception = Assert.Throws<Win32Exception>(() => process.kill(nonExistentPid, 0));
        Assert.Equal(3, exception.NativeErrorCode);
        Assert.Contains("ESRCH", exception.Message);
        Assert.Contains("No such process", exception.Message);
    }

    [Fact]
    public void kill_ShouldReturnTrueForSignalZeroOnOwnProcess()
    {
        var currentPid = process.pid;
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            var exception = Assert.Throws<PlatformNotSupportedException>(() => process.kill(currentPid, 0));
            Assert.Contains("ERR_FEATURE_UNAVAILABLE", exception.Message);
            return;
        }

        var result = process.kill(currentPid, 0);

        Assert.True(result);
    }

    [Fact]
    public void kill_ShouldWorkWithSleepProcess()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            return;
        using var child = startSignalChild("TERM", 41);
        try
        {
            Assert.True(process.kill(child.Id));
            Assert.True(child.WaitForExit(5000));
            Assert.Equal(41, child.ExitCode);
        }
        finally
        {
            reapSignalChild(child);
        }
    }

    [Fact]
    public void killNamed_ShouldDeliverTheRequestedNativeSignal()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            return;
        using var child = startSignalChild("USR1", 37);
        try
        {
            Assert.True(process.killNumber((double)child.Id, 0D));
            Assert.False(child.HasExited);
            Assert.True(process.killNamed((long)child.Id, "SIGUSR1"));
            Assert.True(child.WaitForExit(5000));
            Assert.Equal(37, child.ExitCode);
        }
        finally
        {
            reapSignalChild(child);
        }
    }

    [Fact]
    public void kill_ShouldRejectInvalidNumbersAndNamesWithoutSendingASignal()
    {
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            return;
        using var child = startSignalChild("TERM", 41);
        try
        {
            foreach (var signal in new[] { "BOGUS", "sigterm", "0", "SIGNULL" })
                Assert.Contains("ERR_UNKNOWN_SIGNAL", Assert.Throws<ArgumentException>(
                    () => process.killNamed(child.Id, signal)).Message);
            foreach (var signal in new[] { 0.5, double.NaN, double.PositiveInfinity, double.NegativeInfinity, (double)int.MaxValue + 1 })
            {
                var exception = Assert.Throws<ArgumentException>(() => process.killNumber(child.Id, signal));
                Assert.Contains("ERR_INVALID_ARG_TYPE", exception.Message);
                Assert.Equal("signal", exception.ParamName);
                Assert.IsType<OverflowException>(exception.InnerException);
            }
            foreach (var signal in new[] { -1, int.MaxValue })
                Assert.Contains("ERR_UNKNOWN_SIGNAL", Assert.Throws<ArgumentException>(
                    () => process.killNumber(child.Id, signal)).Message);
            foreach (var pid in new[] { (long)int.MaxValue + 1, long.MaxValue, long.MinValue, 9007199254740993L })
            {
                var exception = Assert.Throws<ArgumentException>(() => process.killNumber(pid, 0));
                Assert.Contains("ERR_INVALID_ARG_TYPE", exception.Message);
                Assert.Equal("pid", exception.ParamName);
                Assert.IsType<OverflowException>(exception.InnerException);
            }
            foreach (var pid in new[] { double.NaN, double.PositiveInfinity, child.Id + 0.5 })
                Assert.Contains("ERR_INVALID_ARG_TYPE", Assert.Throws<ArgumentException>(
                    () => process.killNumber(pid, 0)).Message);
            var wideSignal = Assert.Throws<ArgumentException>(() => process.killNumber(child.Id, ulong.MaxValue));
            Assert.Equal("signal", wideSignal.ParamName);
            Assert.Contains("ERR_INVALID_ARG_TYPE", wideSignal.Message);
            Assert.True(process.killNumber((long)child.Id, 0M));
            Assert.False(child.HasExited);
        }
        finally
        {
            reapSignalChild(child);
        }
    }

    [Fact]
    public void kill_ShouldHaveOnlyTypedInputsAndNoSuccessPathAllocations()
    {
        Assert.Null(typeof(process).GetMethod("kill", new[] { typeof(int), typeof(object) }));
        var native = typeof(process).GetMethod("kill", new[] { typeof(int), typeof(int?) });
        Assert.NotNull(native);
        Assert.Equal(typeof(bool), native.ReturnType);
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
            return;
        var currentPid = process.pid;
        for (var iteration = 0; iteration < 100; iteration++)
            process.kill(currentPid, 0);
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var iteration = 0; iteration < 100; iteration++)
            process.kill(currentPid, 0);
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(0, allocated);
    }

    internal static Process startSignalChild(string signal, int exitCode)
    {
        var start = new ProcessStartInfo
        {
            FileName = "/bin/sh",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        start.ArgumentList.Add("-c");
        start.ArgumentList.Add($"trap 'exit {exitCode}' {signal}; printf 'READY\\n'; while :; do :; done");
        var child = Process.Start(start);
        Assert.NotNull(child);
        try
        {
            var ready = child.StandardOutput.ReadLineAsync();
            Assert.True(ready.Wait(TimeSpan.FromSeconds(5)));
            Assert.Equal("READY", ready.Result);
            return child;
        }
        catch
        {
            reapSignalChild(child);
            child.Dispose();
            throw;
        }
    }

    internal static void reapSignalChild(Process child)
    {
        if (!child.HasExited)
            child.Kill();
        Assert.True(child.WaitForExit(5000));
    }
}
