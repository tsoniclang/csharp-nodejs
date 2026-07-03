using Xunit;
using System;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class ChildProcessRefTests
{
    private static bool IsWindows => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

    [Fact]
    public void ref_RefUnref_UpdatesReferencedProperty()
    {
        var command = IsWindows ? "cmd" : "echo";
        var args = IsWindows ? new[] { "/c", "echo", "test" } : new[] { "test" };
        var child = child_process.spawn(command, args);

        // Default should be referenced
        Assert.True(child.referenced);

        // Unref should set to false
        child.unref();
        Assert.False(child.referenced);

        // Ref should set back to true
        child.@ref();
        Assert.True(child.referenced);
    }
}
