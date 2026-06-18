using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class matchesGlobTests
{
    [Fact]
    public void matchesGlob_ShouldMatchSimplePatterns()
    {
        Assert.True(path.matchesGlob("file.txt", "file.txt"));
        Assert.False(path.matchesGlob("file.txt", "other.txt"));
    }

    [Fact]
    public void matchesGlob_WithWildcard_ShouldMatchPattern()
    {
        Assert.True(path.matchesGlob("file.txt", "*.txt"));
        Assert.True(path.matchesGlob("readme.md", "*.md"));
        Assert.False(path.matchesGlob("file.txt", "*.md"));
    }

    [Fact]
    public void matchesGlob_EmptyStrings_ShouldReturnFalse()
    {
        Assert.False(path.matchesGlob("", ""));
        Assert.False(path.matchesGlob("file.txt", ""));
        Assert.False(path.matchesGlob("", "*.txt"));
    }
}
