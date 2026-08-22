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
    public void LegacyParse_PreservesRelativeUrlShape()
    {
        var parsed = url.parse("images/cover.png?width=320#preview");

        Assert.Null(parsed.protocol);
        Assert.Null(parsed.hostname);
        Assert.Equal("images/cover.png", parsed.pathname);
        Assert.Equal("?width=320", parsed.search);
        Assert.Equal("width=320", parsed.queryText);
        Assert.Equal("#preview", parsed.hash);
        Assert.Equal("images/cover.png?width=320", parsed.path);
        Assert.Equal("images/cover.png?width=320#preview", url.format(parsed));
        parsed.queryText = "height=240";
        Assert.Equal("height=240", parsed.query);
    }

    [Fact]
    public void LegacyParse_HonorsSlashesDenoteHost()
    {
        var path = url.parse("//cdn.example.test/image.png", false, false);
        Assert.Null(path.slashes);
        Assert.Null(path.host);
        Assert.Equal("//cdn.example.test/image.png", path.pathname);

        var authority = url.parse("//cdn.example.test/image.png", false, true);
        Assert.True(authority.slashes == true);
        Assert.Equal("cdn.example.test", authority.host);
        Assert.Equal("cdn.example.test", authority.hostname);
        Assert.Equal("/image.png", authority.pathname);
    }

    [Fact]
    public void LegacyFormat_ComposesSupportedObjectFieldsLikeNode()
    {
        var formatted = url.format(new LegacyUrlObject
        {
            protocol = "https",
            hostname = "example.test",
            port = "8443",
            pathname = "docs",
            search = "page=1",
            hash = "top"
        });

        Assert.Equal("https://example.test:8443/docs?page=1#top", formatted);
        Assert.Equal("file:///tmp/a", url.format(new LegacyUrlObject
        {
            protocol = "file:",
            pathname = "/tmp/a"
        }));
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
