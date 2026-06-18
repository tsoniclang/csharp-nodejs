using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class normalizeTests
{
    [Fact]
    public void normalize_ShouldNormalizePath()
    {
        var result = path.normalize("/foo/bar/../baz");
        Assert.DoesNotContain("..", result);
    }

    [Fact]
    public void normalize_EmptyPath_ShouldReturnDot()
    {
        Assert.Equal(".", path.normalize(""));
    }
}
