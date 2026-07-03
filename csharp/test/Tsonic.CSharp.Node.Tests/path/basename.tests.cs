using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class basenameTests
{
    [Fact]
    public void basename_ShouldReturnFileName()
    {
        Assert.Equal("file.txt", path.basename("/foo/bar/file.txt"));
        Assert.Equal("file.txt", path.basename("file.txt"));

        // Platform-specific test
        if (Path.DirectorySeparatorChar == '\\')
        {
            Assert.Equal("file.txt", path.basename("C:\\foo\\bar\\file.txt"));
        }
    }

    [Fact]
    public void basename_WithSuffix_ShouldRemoveSuffix()
    {
        Assert.Equal("file", path.basename("/foo/bar/file.txt", ".txt"));
        Assert.Equal("file.txt", path.basename("/foo/bar/file.txt", ".html"));
        Assert.Equal("index", path.basename("index.html", ".html"));
    }

    [Fact]
    public void basename_EmptyPath_ShouldReturnEmpty()
    {
        Assert.Equal(string.Empty, path.basename(""));
    }
}
