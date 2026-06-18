using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessExecOptionsTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void ExecOptions_WithEnvVariables_PassesEnvironment()
    {
        var command = IsWindows ? "echo %TEST_VAR%" : "echo $TEST_VAR";
        var options = new ExecOptions
        {
            encoding = "utf8",
            env = new { TEST_VAR = "test_value", PATH = Environment.GetEnvironmentVariable("PATH") }
        };
        var result = child_process.execSync(command, options);

        Assert.IsType<string>(result);
        var output = (string)result;
        Assert.Contains("test_value", output);
    }

    [Fact]
    public void ExecOptions_WithTimeout_TerminatesProcess()
    {
        var command = IsWindows ? "timeout /t 10" : "sleep 10";
        var options = new ExecOptions { timeout = 100 }; // 100ms timeout

        Assert.Throws<TimeoutException>(() =>
        {
            child_process.execSync(command, options);
        });
    }

    [Fact]
    public void ExecOptions_WithTimeout_SetsSignal()
    {
        var command = IsWindows ? "timeout" : "sleep";
        var args = IsWindows ? new[] { "/t", "10" } : new[] { "10" };
        var options = new ExecOptions { timeout = 100 }; // 100ms timeout
        var result = child_process.spawnSync(command, args, options);

        Assert.NotNull(result.signal);
        Assert.Equal("SIGTERM", result.signal);
        Assert.NotNull(result.error);
        Assert.IsType<TimeoutException>(result.error);
    }

    [Fact]
    public void ExecOptions_AllProperties_CanBeSet()
    {
        var options = new ExecOptions
        {
            cwd = "/tmp",
            env = new { TEST = "value" },
            encoding = "utf8",
            shell = "/bin/sh",
            timeout = 1000,
            maxBuffer = 2048,
            killSignal = "SIGKILL",
            windowsHide = true,
            windowsVerbatimArguments = false,
            detached = false,
            uid = 1000,
            gid = 1000,
            argv0 = "test",
            stdio = "pipe",
            input = "test input"
        };

        Assert.Equal("/tmp", options.cwd);
        Assert.NotNull(options.env);
        Assert.Equal("utf8", options.encoding);
        Assert.Equal("/bin/sh", options.shell);
        Assert.Equal(1000, options.timeout);
        Assert.Equal(2048, options.maxBuffer);
        Assert.Equal("SIGKILL", options.killSignal);
        Assert.True(options.windowsHide);
        Assert.False(options.windowsVerbatimArguments);
        Assert.False(options.detached);
        Assert.Equal(1000, options.uid);
        Assert.Equal(1000, options.gid);
        Assert.Equal("test", options.argv0);
        Assert.Equal("pipe", options.stdio);
        Assert.Equal("test input", options.input);
    }

    [Fact]
    public void ExecOptions_WithMaxBuffer_RespectedInOptions()
    {
        var command = IsWindows ? "echo test" : "echo 'test'";
        var options = new ExecOptions
        {
            encoding = "utf8",
            maxBuffer = 1024 * 10 // 10KB
        };
        var result = child_process.execSync(command, options);

        Assert.IsType<string>(result);
    }
}
