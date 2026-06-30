using System.Text;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public partial class StringDecoderTests
{
    [Fact]
    public void write_ShouldDecodeSimpleUtf8String()
    {
        var decoder = new StringDecoder("utf8");
        var bytes = Encoding.UTF8.GetBytes("hello world");

        var result = decoder.write(bytes);

        Assert.Equal("hello world", result);
    }

    [Fact]
    public void write_ShouldHandleEmptyBuffer()
    {
        var decoder = new StringDecoder("utf8");
        var bytes = new byte[0];

        var result = decoder.write(bytes);

        Assert.Equal("", result);
    }

    [Fact]
    public void write_ShouldHandleMultipleCompleteCharacters()
    {
        var decoder = new StringDecoder("utf8");

        // Test with multiple multi-byte characters
        var bytes = Encoding.UTF8.GetBytes("Hello 世界");

        var result = decoder.write(bytes);

        Assert.Equal("Hello 世界", result);
    }

    [Fact]
    public void write_ShouldHandleAsciiEncoding()
    {
        var decoder = new StringDecoder("ascii");
        var bytes = Encoding.ASCII.GetBytes("hello");

        var result = decoder.write(bytes);

        Assert.Equal("hello", result);
    }

    [Fact]
    public void write_ShouldHandleLatin1Encoding()
    {
        var decoder = new StringDecoder("latin1");
        var bytes = Encoding.Latin1.GetBytes("café");

        var result = decoder.write(bytes);

        Assert.Equal("café", result);
    }

    [Fact]
    public void write_ShouldDefaultToUtf8()
    {
        var decoder = new StringDecoder();
        var bytes = Encoding.UTF8.GetBytes("hello");

        var result = decoder.write(bytes);

        Assert.Equal("hello", result);
    }

    [Fact]
    public void utf16_ShouldDecodeCorrectly()
    {
        var decoder = new StringDecoder("utf16le");
        var bytes = Encoding.Unicode.GetBytes("hello");

        var result = decoder.write(bytes);

        Assert.Equal("hello", result);
    }

    [Fact]
    public void write_ShouldHandleNullBuffer()
    {
        var decoder = new StringDecoder("utf8");

        var result = decoder.write(null!);

        Assert.Equal("", result);
    }

    // === UTF-16LE Incomplete Sequence Tests ===

    [Fact]
    public void write_Utf16le_IncompleteSurrogatePair()
    {
        var decoder = new StringDecoder("utf16le");

        // 𝄞 in UTF-16LE requires surrogate pair: 0x34 0xD8 0x1E 0xDD
        var result1 = decoder.write(new byte[] { 0x34, 0xD8 }); // High surrogate incomplete
        Assert.Equal("", result1); // Should buffer

        var result2 = decoder.write(new byte[] { 0x1E, 0xDD }); // Low surrogate
        Assert.Equal("𝄞", result2);
    }

    [Fact]
    public void write_Utf16le_SplitSingleByte()
    {
        var decoder = new StringDecoder("utf16le");

        // "A" in UTF-16LE = 0x41 0x00
        var result1 = decoder.write(new byte[] { 0x41 });
        Assert.Equal("", result1); // Incomplete

        var result2 = decoder.write(new byte[] { 0x00 });
        Assert.Equal("A", result2); // Complete
    }
}
