using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class pathModuleTests
{
    [Fact]
    public void sep_ShouldReturnPlatformSeparator()
    {
        Assert.Equal(Path.DirectorySeparatorChar.ToString(), path.sep);
    }

    [Fact]
    public void delimiter_ShouldReturnPlatformDelimiter()
    {
        Assert.Equal(Path.PathSeparator.ToString(), path.delimiter);
    }

    [Fact]
    public void posixAndWin32_ShouldExist()
    {
        Assert.NotNull(path.posix);
        Assert.NotNull(path.win32);
        Assert.Same(PathModule.Instance, path.posix);
        Assert.Same(PathModule.Instance, path.win32);
    }

    [Fact]
    public void pathModule_ShouldDelegateToPaths()
    {
        var module = PathModule.Instance;

        Assert.Equal(path.sep, module.sep);
        Assert.Equal(path.delimiter, module.delimiter);
        Assert.Equal(path.basename("file.txt"), module.basename("file.txt"));
        Assert.Equal(path.dirname("/foo/bar"), module.dirname("/foo/bar"));
        Assert.Equal(path.extname("file.txt"), module.extname("file.txt"));
    }
}
