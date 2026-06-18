using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessNoArgsTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void noArgs_SpawnSync_Works()
    {
        var command = IsWindows ? "cmd" : "pwd";
        var result = child_process.spawnSync(command);

        Assert.NotNull(result);
        Assert.True(result.pid > 0);
    }

    [Fact]
    public void noArgs_ExecFileSync_Works()
    {
        var file = IsWindows ? "cmd.exe" : "/bin/pwd";
        var result = child_process.execFileSync(file);

        Assert.NotNull(result);
    }
}
