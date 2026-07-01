using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class utilExtrasTests
{
    [Fact]
    public void formatWithOptions_ShouldRejectOpenCarrierSemantics()
    {
        var exception = Assert.Throws<NotSupportedException>(() => util.formatWithOptions(new { colors = true }, "hello %s", "world"));
        Assert.Contains("node:util.formatWithOptions", exception.Message);
        Assert.Contains("closed provider/runtime carrier semantics", exception.Message);
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
