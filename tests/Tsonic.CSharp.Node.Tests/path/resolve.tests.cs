using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class Path_resolveTests
{
    [Fact]
    public void resolve_ShouldResolveToAbsolutePath()
    {
        var result = path.resolve("foo", "bar");
        Assert.True(Path.IsPathRooted(result));
        Assert.Contains("foo", result);
        Assert.Contains("bar", result);
    }

    [Fact]
    public void resolve_EmptyPaths_ShouldReturnCurrentDirectory()
    {
        var result = path.resolve();
        Assert.Equal(Directory.GetCurrentDirectory(), result);
    }
}
