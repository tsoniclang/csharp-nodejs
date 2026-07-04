using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class cwdTests
{
    [Fact]
    public void cwd_ShouldReturnCurrentDirectory()
    {
        var cwd = process.cwd();

        Assert.NotNull(cwd);
        Assert.NotEmpty(cwd);
    }

    [Fact]
    public void cwd_ShouldMatchDirectoryGetCurrentDirectory()
    {
        var cwd = process.cwd();
        var expected = Directory.GetCurrentDirectory();

        Assert.Equal(expected, cwd);
    }

    [Fact]
    public void cwd_ShouldBeAbsolutePath()
    {
        var cwd = process.cwd();

        Assert.True(Path.IsPathRooted(cwd));
    }

    [Fact]
    public void cwd_ShouldExist()
    {
        var cwd = process.cwd();

        Assert.True(Directory.Exists(cwd));
    }
}
