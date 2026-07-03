using System.Collections.Generic;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class QueryStringOptionsTests
{
    [Fact]
    public void ParseOptions_ShouldUseCustomDecoderAndMaxKeys()
    {
        var result = querystring.parse("a=x&b=y", "&", "=", new ParseOptions
        {
            maxKeys = 1,
            decodeURIComponent = value => value.ToUpperInvariant()
        });

        Assert.Single(result);
        Assert.Equal("X", result["A"]);
    }

    [Fact]
    public void StringifyOptions_ShouldUseCustomEncoder()
    {
        var result = querystring.stringify(new Dictionary<string, object?>
        {
            ["a b"] = "c d"
        }, "&", "=", new StringifyOptions
        {
            encodeURIComponent = value => value.Replace(" ", "+", System.StringComparison.Ordinal)
        });

        Assert.Equal("a+b=c+d", result);
    }

    [Fact]
    public void UnescapeBuffer_ShouldReturnDecodedUtf8Bytes()
    {
        var bytes = querystring.unescapeBuffer("hello%20world");

        Assert.Equal("hello world", System.Text.Encoding.UTF8.GetString(bytes));
    }
}
