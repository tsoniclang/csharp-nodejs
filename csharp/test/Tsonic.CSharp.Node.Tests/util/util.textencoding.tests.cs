using System.Text;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class UtilTextEncodingTests
{
    [Fact]
    public void TextEncoder_ShouldEncodeUtf8AndEncodeInto()
    {
        var encoder = new TextEncoder();
        var bytes = encoder.encode("hello");
        var destination = new byte[3];
        var result = encoder.encodeInto("hello", destination);

        Assert.Equal("utf-8", encoder.encoding);
        Assert.Equal(Encoding.UTF8.GetBytes("hello"), bytes);
        Assert.Equal(3, result.written);
        Assert.Equal(3, result.read);
    }

    [Fact]
    public void TextDecoder_ShouldDecodeUtf8AndHonorBom()
    {
        var decoder = new TextDecoder();
        var result = decoder.decode([0xEF, 0xBB, 0xBF, 0x61]);

        Assert.Equal("utf-8", decoder.encoding);
        Assert.Equal("a", result);
    }

    [Fact]
    public void TextDecoder_ShouldSupportLatin1()
    {
        var decoder = new TextDecoder("latin1");

        Assert.Equal("windows-1252", decoder.encoding);
        Assert.Equal("é", decoder.decode([0xE9]));
    }
}
