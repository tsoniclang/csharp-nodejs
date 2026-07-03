using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class relativeTests
{
    [Fact]
    public void relative_ShouldCalculateRelativePath()
    {
        // Create absolute paths for testing
        var from = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "foo", "bar"));
        var to = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "foo", "baz"));

        var result = path.relative(from, to);
        Assert.NotEmpty(result);
        // Should not be an absolute path
        Assert.False(Path.IsPathRooted(result));
    }

    [Fact]
    public void relative_SamePath_ShouldReturnEmpty()
    {
        var samePath = Path.GetFullPath("foo");
        Assert.Equal(string.Empty, path.relative(samePath, samePath));
    }
}
