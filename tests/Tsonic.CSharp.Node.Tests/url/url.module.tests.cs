using System;
using System.IO;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class urlModuleTests
{
    [Fact]
    public void domainToASCII_ShouldConvertUnicodeDomain()
    {
        var ascii = url.domainToASCII("mañana.com");
        Assert.StartsWith("xn--", ascii);
    }

    [Fact]
    public void pathToFileURL_And_fileURLToPath_ShouldRoundTrip()
    {
        var path = Path.Combine(Path.GetTempPath(), $"url-test-{Guid.NewGuid()}.txt");
        var fileUrl = url.pathToFileURL(path);
        var restored = url.fileURLToPath(fileUrl);
        Assert.Equal(path, restored);
    }

    [Fact]
    public void parse_And_format_ShouldWork()
    {
        var parsed = url.parse("https://example.com/path?q=1");
        Assert.NotNull(parsed);

        var formatted = url.format(parsed);
        Assert.Contains("https://example.com/path", formatted);
    }

    [Fact]
    public void resolve_ShouldBuildAbsoluteUrl()
    {
        var resolved = url.resolve("https://example.com/base/", "../x");
        Assert.Equal("https://example.com/x", resolved);
    }

    [Fact]
    public void urlToHttpOptions_ShouldExposeFields()
    {
        var options = url.urlToHttpOptions(new URL("https://example.com:8080/path?q=1"));
        Assert.Equal("https:", options["protocol"]);
        Assert.Equal("example.com", options["hostname"]);
        Assert.Equal("/path?q=1", options["path"]);
    }
}
