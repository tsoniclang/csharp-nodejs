using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class joinTests
{
    [Fact]
    public void join_ShouldJoinPaths()
    {
        var result = path.join("/foo", "bar", "baz");
        Assert.Contains("foo", result);
        Assert.Contains("bar", result);
        Assert.Contains("baz", result);
    }

    [Fact]
    public void join_EmptyPaths_ShouldReturnDot()
    {
        Assert.Equal(".", path.join());
        Assert.Equal(".", path.join("", ""));
    }

    [Fact]
    public void join_FilterEmptySegments()
    {
        var result = path.join("/foo", "", "bar");
        Assert.Contains("foo", result);
        Assert.Contains("bar", result);
    }
}
