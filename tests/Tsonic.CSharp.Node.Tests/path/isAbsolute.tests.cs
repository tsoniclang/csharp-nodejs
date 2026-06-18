using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class isAbsoluteTests
{
    [Fact]
    public void isAbsolute_ShouldDetectAbsolutePaths()
    {
        // Platform-specific behavior
        if (Path.DirectorySeparatorChar == '/')
        {
            // Unix
            Assert.True(path.isAbsolute("/foo/bar"));
            Assert.False(path.isAbsolute("foo/bar"));
            Assert.False(path.isAbsolute("./foo"));
        }
        else
        {
            // Windows
            Assert.True(path.isAbsolute("C:\\foo\\bar"));
            Assert.False(path.isAbsolute("foo\\bar"));
            Assert.False(path.isAbsolute(".\\foo"));
        }
    }

    [Fact]
    public void isAbsolute_EmptyPath_ShouldReturnFalse()
    {
        Assert.False(path.isAbsolute(""));
    }
}
