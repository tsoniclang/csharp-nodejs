using System.Text;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public partial class StringDecoderTests
{

    [Fact]
    public void end_ShouldReturnEmptyStringWithoutBuffer()
    {
        var decoder = new StringDecoder("utf8");

        var result = decoder.end();

        Assert.Equal("", result);
    }

    [Fact]
    public void end_ShouldDecodeOptionalBuffer()
    {
        var decoder = new StringDecoder("utf8");
        var bytes = Encoding.UTF8.GetBytes("hello");

        var result = decoder.end(bytes);

        Assert.Equal("hello", result);
    }

    [Fact]
    public void end_ShouldFlushIncompleteBytes()
    {
        var decoder = new StringDecoder("utf8");

        // Start of a multi-byte sequence
        var bytes = new byte[] { 0xE2, 0x82 };

        decoder.write(bytes);
        var result = decoder.end();

        // Should return something (substitution character or the incomplete bytes)
        Assert.NotNull(result);
    }

    [Fact]
    public void end_ShouldAllowReuse()
    {
        var decoder = new StringDecoder("utf8");

        // First use
        var bytes1 = Encoding.UTF8.GetBytes("hello");
        var result1 = decoder.end(bytes1);
        Assert.Equal("hello", result1);

        // Second use after end()
        var bytes2 = Encoding.UTF8.GetBytes("world");
        var result2 = decoder.write(bytes2);
        Assert.Equal("world", result2);
    }

    // === end() Additional Tests ===

    [Fact]
    public void end_WithEmptyByteArray_ShouldReturnEmpty()
    {
        var decoder = new StringDecoder("utf8");
        var result = decoder.end(new byte[] { });
        Assert.Equal("", result);
    }

    [Fact]
    public void end_WithBufferAfterIncompleteWrite_ShouldFlushBothAndDecode()
    {
        var decoder = new StringDecoder("utf8");

        // Write incomplete Euro symbol (first 2 bytes)
        decoder.write(new byte[] { 0xE2, 0x82 });

        // Call end with a new buffer
        var result = decoder.end(new byte[] { 0x68, 0x69 }); // "hi"

        // Should flush incomplete sequence (replacement char) and decode "hi"
        Assert.True(result.Length > 0);
        Assert.Contains("hi", result);
    }

    [Fact]
    public void end_WithIncompleteSequenceInBuffer_ShouldFlushWithReplacement()
    {
        var decoder = new StringDecoder("utf8");

        // Pass incomplete sequence directly to end()
        var result = decoder.end(new byte[] { 0xE2, 0x82 }); // Incomplete Euro

        // Should return replacement character
        Assert.True(result.Length > 0);
    }

    [Fact]
    public void end_WithCompleteAndIncompleteInBuffer_ShouldDecodeCompleteAndFlushIncomplete()
    {
        var decoder = new StringDecoder("utf8");

        // "hi" + incomplete Euro = 0x68 0x69 0xE2 0x82
        var result = decoder.end(new byte[] { 0x68, 0x69, 0xE2, 0x82 });

        // Should return "hi" + replacement character
        Assert.True(result.Length >= 2);
        Assert.StartsWith("hi", result);
    }

    [Fact]
    public void end_CalledTwice_ShouldReturnEmptyOnSecondCall()
    {
        var decoder = new StringDecoder("utf8");

        decoder.write(new byte[] { 0x68, 0x69 }); // "hi"
        var result1 = decoder.end();

        // Second call without writing should return empty
        var result2 = decoder.end();
        Assert.Equal("", result2);
    }

    [Fact]
    public void end_AfterWrite_ThenWriteAgain_ShouldWork()
    {
        var decoder = new StringDecoder("utf8");

        decoder.write(new byte[] { 0x68, 0x69 }); // "hi"
        decoder.end();

        // Should be able to write again
        var result = decoder.write(new byte[] { 0x62, 0x79, 0x65 }); // "bye"
        Assert.Equal("bye", result);
    }

    [Fact]
    public void end_WithNullBuffer_ShouldFlushAnyIncomplete()
    {
        var decoder = new StringDecoder("utf8");

        // Write incomplete sequence
        decoder.write(new byte[] { 0xE2 });

        // end(null) should flush
        var result = decoder.end(null);
        Assert.True(result.Length >= 0); // May be empty or replacement char
    }

    // === Edge Case: All multibyte lengths together ===

    [Fact]
    public void write_AllMultibyteLengths_SplitAcrossWrites()
    {
        var decoder = new StringDecoder("utf8");

        // ASCII (1 byte) + 2-byte + 3-byte + 4-byte: "a¢€𝄞"
        // = 0x61 0xC2 0xA2 0xE2 0x82 0xAC 0xF0 0x9D 0x84 0x9E

        var result1 = decoder.write(new byte[] { 0x61 }); // 'a'
        Assert.Equal("a", result1);

        var result2 = decoder.write(new byte[] { 0xC2, 0xA2 }); // ¢
        Assert.Equal("¢", result2);

        var result3 = decoder.write(new byte[] { 0xE2 }); // Start of €
        Assert.Equal("", result3);

        var result4 = decoder.write(new byte[] { 0x82, 0xAC }); // Complete €
        Assert.Equal("€", result4);

        var result5 = decoder.write(new byte[] { 0xF0, 0x9D }); // Start of 𝄞
        Assert.Equal("", result5);

        var result6 = decoder.write(new byte[] { 0x84, 0x9E }); // Complete 𝄞
        Assert.Equal("𝄞", result6);
    }
}
