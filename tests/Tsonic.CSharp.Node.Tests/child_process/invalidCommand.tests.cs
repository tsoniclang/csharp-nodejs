using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessInvalidCommandTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void invalidCommand_ThrowsException()
    {
        // Invalid commands should throw when process exits with non-zero code
        var command = IsWindows ? "exit /b 99" : "exit 99";

        Assert.Throws<InvalidOperationException>(() =>
        {
            child_process.execSync(command);
        });
    }
}
