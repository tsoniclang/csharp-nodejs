using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class dirnameTests
{
    [Fact]
    public void dirname_ShouldReturnDirectory()
    {
        Assert.Equal("/foo/bar", path.dirname("/foo/bar/baz"));
        Assert.Equal("/foo", path.dirname("/foo/bar"));
        Assert.Equal(".", path.dirname("file.txt"));
    }

    [Fact]
    public void dirname_EmptyPath_ShouldReturnDot()
    {
        Assert.Equal(".", path.dirname(""));
    }
}
