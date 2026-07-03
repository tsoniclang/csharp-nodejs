using Xunit;
using System.Diagnostics;

namespace Tsonic.CSharp.Node.Tests;

public class killTests
{
    [Fact]
    public void kill_ShouldThrowForNonExistentProcess()
    {
        // Use a PID that's unlikely to exist (very high number)
        var nonExistentPid = 999999;

        var exception = Assert.Throws<Exception>(() => process.kill(nonExistentPid));
        Assert.Contains("ESRCH", exception.Message);
        Assert.Contains("No such process", exception.Message);
    }

    [Fact]
    public void kill_ShouldReturnTrueForSignalZeroOnOwnProcess()
    {
        // Signal 0 can be used to check if a process exists without killing it
        var currentPid = process.pid;

        var result = process.kill(currentPid, 0);

        Assert.True(result);
    }

    [Fact]
    public void kill_ShouldWorkWithSleepProcess()
    {
        // Create a long-running process that we can safely kill
        // Using 'sleep' command which is available on Unix systems
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "sleep",
                Arguments = "60",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var testProcess = Process.Start(startInfo);
            Assert.NotNull(testProcess);

            var pid = testProcess.Id;

            // Give it a moment to start
            Thread.Sleep(100);

            // Kill it
            var result = process.kill(pid);

            Assert.True(result);

            // Wait for it to exit
            testProcess.WaitForExit(1000);

            Assert.True(testProcess.HasExited);
        }
    }
}
