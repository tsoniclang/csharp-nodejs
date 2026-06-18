using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class argvTests
{
    [Fact]
    public void argv_ShouldReturnArray()
    {
        var argv = process.argv;

        Assert.NotNull(argv);
    }

    [Fact]
    public void argv0_ShouldReturnString()
    {
        var argv0 = process.argv0;

        Assert.NotNull(argv0);
    }

    [Fact]
    public void argv_ShouldBeSettable()
    {
        var original = process.argv;
        var newArgv = new[] { "test", "args" };

        process.argv = newArgv;

        Assert.Equal(newArgv, process.argv);

        // Restore original
        process.argv = original;
    }

    [Fact]
    public void argv0_ShouldBeSettable()
    {
        var original = process.argv0;
        var newArgv0 = "test-executable";

        process.argv0 = newArgv0;

        Assert.Equal(newArgv0, process.argv0);

        // Restore original
        process.argv0 = original;
    }

    [Fact]
    public void argv_ShouldHandleNullByReturningEmptyArray()
    {
        var original = process.argv;

        process.argv = null!;

        Assert.NotNull(process.argv);
        Assert.Empty(process.argv);

        // Restore original
        process.argv = original;
    }

    [Fact]
    public void argv0_ShouldHandleNullByReturningEmptyString()
    {
        var original = process.argv0;

        process.argv0 = null!;

        Assert.Equal(string.Empty, process.argv0);

        // Restore original
        process.argv0 = original;
    }
}
