using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class formatTests
{
    [Fact]
    public void format_ShouldFormatPathFromObject()
    {
        var pathObject = new ParsedPath
        {
            dir = "/home/user/dir",
            @base = "file.txt"
        };

        var result = path.format(pathObject);
        Assert.Contains("file.txt", result);
    }

    [Fact]
    public void format_NullObject_ShouldReturnEmpty()
    {
        Assert.Equal(string.Empty, path.format(null!));
    }

    [Fact]
    public void format_OnlyBase_ShouldReturnBase()
    {
        var pathObject = new ParsedPath
        {
            @base = "file.txt"
        };

        var result = path.format(pathObject);
        Assert.Equal("file.txt", result);
    }
}
