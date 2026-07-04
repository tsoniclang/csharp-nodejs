using System.IO;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class ConsoleOptionsTests
{
    [Fact]
    public void ConsoleOptions_ShouldCarryStreamsAndColorMode()
    {
        var stdout = new StringWriter();
        var options = new ConsoleOptions
        {
            stdout = stdout,
            colorMode = ConsoleColorMode.always,
            ignoreErrors = false
        };

        Assert.Same(stdout, options.stdout);
        Assert.Equal(ConsoleColorMode.always, options.colorMode);
        Assert.False(options.ignoreErrors);
    }
}
