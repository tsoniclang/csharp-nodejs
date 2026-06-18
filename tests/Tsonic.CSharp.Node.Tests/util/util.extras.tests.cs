using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class utilExtrasTests
{
    [Fact]
    public void formatWithOptions_ShouldFormat()
    {
        var result = util.formatWithOptions(new { colors = true }, "hello %s", "world");
        Assert.Equal("hello world", result);
    }

    [Fact]
    public void stripVTControlCharacters_ShouldRemoveAnsiSequences()
    {
        var result = util.stripVTControlCharacters("\x1B[31mred\x1B[0m");
        Assert.Equal("red", result);
    }

    [Fact]
    public void toUSVString_ShouldReplaceLoneSurrogates()
    {
        var input = "a\uD800b";
        var result = util.toUSVString(input);
        Assert.Equal("a\uFFFDb", result);
    }
}
