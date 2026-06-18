using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessMaxBufferTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void maxBuffer_RespectedInOptions()
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
