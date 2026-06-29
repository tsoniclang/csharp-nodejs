using System.Linq;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class UtilMimeTests
{
    [Fact]
    public void MIMEParams_ShouldReadWriteAndSerialize()
    {
        var parameters = new MIMEParams();

        parameters.set("charset", "utf-8");
        parameters.set("boundary", "a b");

        Assert.True(parameters.has("CHARSET"));
        Assert.Equal("utf-8", parameters.get("charset"));
        Assert.Equal(["charset", "boundary"], parameters.keys().ToArray());
        Assert.Contains("boundary=\"a b\"", parameters.ToString());

        parameters.delete("charset");
        Assert.False(parameters.has("charset"));
    }

    [Fact]
    public void MIMEType_ShouldParseAndExposeEssence()
    {
        var type = new MIMEType("Text/HTML; Charset=UTF-8");

        Assert.Equal("text/html", type.essence);
        Assert.Equal("text", type.type);
        Assert.Equal("html", type.subtype);
        Assert.Equal("UTF-8", type.@params.get("charset"));

        type.@params.set("q", "1");
        Assert.Contains("q=1", type.ToString());
    }
}
