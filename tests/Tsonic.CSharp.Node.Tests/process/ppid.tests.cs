using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class ppidTests
{
    [Fact]
    public void ppid_ShouldReturnNonNegativeValue()
    {
        var ppid = process.ppid;

        Assert.True(ppid >= 0);
    }

    [Fact]
    public void ppid_ShouldBeDifferentFromPid()
    {
        var pid = process.pid;
        var ppid = process.ppid;

        // Parent PID should be different from current PID
        // (unless ppid returns 0 on Windows where it's not implemented)
        if (ppid > 0)
        {
            Assert.NotEqual(pid, ppid);
        }
    }
}
