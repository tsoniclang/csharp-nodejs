using Xunit;
using System.Diagnostics;

namespace Tsonic.CSharp.Node.Tests;

public class pidTests
{
    [Fact]
    public void pid_ShouldReturnValidProcessId()
    {
        var pid = process.pid;

        Assert.True(pid > 0);
    }

    [Fact]
    public void pid_ShouldMatchEnvironmentProcessId()
    {
        var pid = process.pid;
        var expected = Environment.ProcessId;

        Assert.Equal(expected, pid);
    }

    [Fact]
    public void pid_ShouldMatchCurrentProcessId()
    {
        var pid = process.pid;
        var expected = Process.GetCurrentProcess().Id;

        Assert.Equal(expected, pid);
    }
}
