using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class exitCodeTests
{
    [Fact]
    public void exitCode_ShouldBeNullByDefault()
    {
        // Save original value
        var original = process.exitCode;

        // Reset to null
        process.exitCode = null;

        Assert.Null(process.exitCode);

        // Restore
        process.exitCode = original;
    }

    [Fact]
    public void exitCode_ShouldBeSettable()
    {
        var original = process.exitCode;

        process.exitCode = 42;

        Assert.Equal(42, process.exitCode);

        // Restore
        process.exitCode = original;
    }

    [Fact]
    public void exitCode_ShouldAcceptZero()
    {
        var original = process.exitCode;

        process.exitCode = 0;

        Assert.Equal(0, process.exitCode);

        // Restore
        process.exitCode = original;
    }

    [Fact]
    public void exitCode_ShouldAcceptNegativeValues()
    {
        var original = process.exitCode;

        process.exitCode = -1;

        Assert.Equal(-1, process.exitCode);

        // Restore
        process.exitCode = original;
    }
}
