using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class UrlMoreTests
{
    [Fact]
    public void LegacyParse_ShouldProduceUrlObjectWithParsedQuery()
    {
        var parsed = url.parse("https://user:pass@example.com/a?x=1&x=2#h", true);

        Assert.NotNull(parsed);
        Assert.Equal("https:", parsed!.protocol);
        Assert.Equal("user:pass", parsed.auth);
        Assert.Equal("/a?x=1&x=2", parsed.path);
        Assert.NotNull(parsed.query);
    }

    [Fact]
    public void FormatOptions_ShouldRemoveSearchAndFragment()
    {
        var formatted = url.format(new URL("https://example.com/a?x=1#h"), new URLFormatOptions
        {
            search = false,
            fragment = false
        });

        Assert.Equal("https://example.com/a", formatted);
    }

    [Fact]
    public void UrlToHttpOptionsObject_ShouldExposeTypedCarrier()
    {
        var options = url.urlToHttpOptionsObject(new URL("https://example.com:8443/a?x=1"));

        Assert.Equal("https:", options.protocol);
        Assert.Equal("example.com", options.hostname);
        Assert.Equal(8443, options.port);
        Assert.Equal("/a?x=1", options.path);
    }

    [Fact]
    public void URLPattern_ShouldSupportInitIgnoreCaseAndExecCarrier()
    {
        var pattern = new URLPattern(new URLPatternInit { baseURL = "https://example.com/*" }, new URLPatternOptions { ignoreCase = true });

        Assert.True(pattern.test("HTTPS://EXAMPLE.COM/a"));
        var result = pattern.exec("https://example.com/a");

        Assert.NotNull(result);
        Assert.Equal(["https://example.com/a"], result!.inputs);
        Assert.Equal("https://example.com/a", result.pathname.input);
    }
}
