using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class bufferModuleTests
{
    [Fact]
    public void btoa_And_atob_ShouldRoundTrip()
    {
        var encoded = buffer.btoa("hello");
        var decoded = buffer.atob(encoded);
        Assert.Equal("hello", decoded);
    }

    [Fact]
    public void isAscii_ShouldValidateBuffer()
    {
        Assert.True(buffer.isAscii(Buffer.from("hello")));
        Assert.False(buffer.isAscii(Buffer.from("héllo")));
    }

    [Fact]
    public void isUtf8_ShouldValidateBytes()
    {
        Assert.True(buffer.isUtf8(Buffer.from("hello")));
        Assert.False(buffer.isUtf8(new byte[] { 0xFF, 0xFF }));
    }

    [Fact]
    public void transcode_ShouldReturnBuffer()
    {
        var result = buffer.transcode(Buffer.from("hello"), "utf8", "utf8");
        Assert.Equal("hello", result.toString());
    }

    [Fact]
    public void constants_ShouldBeAvailable()
    {
        Assert.True(buffer.kMaxLength > 0);
        Assert.True(buffer.kStringMaxLength > 0);
        Assert.True(buffer.constants.MAX_LENGTH > 0);
        Assert.True(buffer.constants.MAX_STRING_LENGTH > 0);
    }
}
