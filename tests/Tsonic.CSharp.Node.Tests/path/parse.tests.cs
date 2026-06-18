using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class parseTests
{
    [Fact]
    public void parse_ShouldParsePathComponents()
    {
        var result = path.parse("/home/user/dir/file.txt");

        Assert.Equal("file.txt", result.@base);
        Assert.Equal(".txt", result.ext);
        Assert.Equal("file", result.name);
        Assert.NotEmpty(result.dir);
    }

    [Fact]
    public void parse_EmptyPath_ShouldReturnEmptyComponents()
    {
        var result = path.parse("");

        Assert.Equal(string.Empty, result.root);
        Assert.Equal(string.Empty, result.dir);
        Assert.Equal(string.Empty, result.@base);
        Assert.Equal(string.Empty, result.ext);
        Assert.Equal(string.Empty, result.name);
    }
}
