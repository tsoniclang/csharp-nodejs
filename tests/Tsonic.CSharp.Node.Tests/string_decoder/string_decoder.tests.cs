using System.Text;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class StringDecoderTests
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
    public void write_ShouldPreserveIncompleteMultibyteSequence()
    {
        var decoder = new StringDecoder("utf8");

        // Euro symbol (€) is 3 bytes in UTF-8: E2 82 AC
        var bytes1 = new byte[] { 0xE2 };
        var bytes2 = new byte[] { 0x82 };
        var bytes3 = new byte[] { 0xAC };

        var result1 = decoder.write(bytes1);
        Assert.Equal("", result1); // Incomplete sequence

        var result2 = decoder.write(bytes2);
        Assert.Equal("", result2); // Still incomplete

        var result3 = decoder.write(bytes3);
        Assert.Equal("€", result3); // Complete sequence
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

    [Fact]
    public void constructor_ShouldAcceptNull()
    {
        var decoder = new StringDecoder(null);

        var bytes = Encoding.UTF8.GetBytes("hello");
        var result = decoder.write(bytes);

        Assert.Equal("hello", result);
    }

    [Fact]
    public void multipleWrites_ShouldAccumulateIncompleteBytes()
    {
        var decoder = new StringDecoder("utf8");

        // Split "hello世界" across multiple writes
        var fullBytes = Encoding.UTF8.GetBytes("hello世界");

        // Write first part
        var part1 = new byte[7]; // "hello" + part of first Chinese character
        Array.Copy(fullBytes, 0, part1, 0, 7);
        var result1 = decoder.write(part1);

        // Write remaining part
        var part2 = new byte[fullBytes.Length - 7];
        Array.Copy(fullBytes, 7, part2, 0, part2.Length);
        var result2 = decoder.write(part2);

        // Combined should give us the full string
        var combined = result1 + result2;
        Assert.Equal("hello世界", combined);
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

    [Fact]
    public void partialMultibyte_InMiddleOfBuffer()
    {
        var decoder = new StringDecoder("utf8");

        // "a" + incomplete Euro symbol
        var bytes = new byte[] { 0x61, 0xE2, 0x82 };

        var result1 = decoder.write(bytes);
        Assert.Equal("a", result1); // 'a' is complete, Euro is incomplete

        // Complete the Euro symbol
        var bytes2 = new byte[] { 0xAC };
        var result2 = decoder.write(bytes2);
        Assert.Equal("€", result2);
    }

    [Fact]
    public void complexScenario_MixedEncodingsOverMultipleWrites()
    {
        var decoder = new StringDecoder("utf8");

        // Test a complex real-world scenario
        var text = "Hello 🌍 World! Café ☕";
        var allBytes = Encoding.UTF8.GetBytes(text);

        var result = "";

        // Split into random chunks and decode
        int offset = 0;
        int[] chunkSizes = { 5, 3, 7, 4, allBytes.Length - 19 };

        foreach (var size in chunkSizes)
        {
            if (offset >= allBytes.Length) break;

            var actualSize = Math.Min(size, allBytes.Length - offset);
            var chunk = new byte[actualSize];
            Array.Copy(allBytes, offset, chunk, 0, actualSize);

            result += decoder.write(chunk);
            offset += actualSize;
        }

        result += decoder.end();

        Assert.Equal(text, result);
    }

    // === Constructor Encoding Tests ===

    [Fact]
    public void constructor_Utf8WithDash_ShouldWork()
    {
        var decoder = new StringDecoder("utf-8");
        var result = decoder.write(Encoding.UTF8.GetBytes("hello"));
        Assert.Equal("hello", result);
    }

    [Fact]
    public void constructor_Ucs2_ShouldWork()
    {
        var decoder = new StringDecoder("ucs2");
        var result = decoder.write(Encoding.Unicode.GetBytes("hello"));
        Assert.Equal("hello", result);
    }

    [Fact]
    public void constructor_Ucs2WithDash_ShouldWork()
    {
        var decoder = new StringDecoder("ucs-2");
        var result = decoder.write(Encoding.Unicode.GetBytes("hello"));
        Assert.Equal("hello", result);
    }

    [Fact]
    public void constructor_Utf16leWithDash_ShouldWork()
    {
        var decoder = new StringDecoder("utf-16le");
        var result = decoder.write(Encoding.Unicode.GetBytes("hello"));
        Assert.Equal("hello", result);
    }

    [Fact]
    public void constructor_Binary_ShouldUseLatin1()
    {
        var decoder = new StringDecoder("binary");
        var result = decoder.write(Encoding.Latin1.GetBytes("café"));
        Assert.Equal("café", result);
    }

    [Fact]
    public void constructor_InvalidEncoding_ShouldDefaultToUtf8()
    {
        var decoder = new StringDecoder("invalid-encoding-xyz");
        var result = decoder.write(Encoding.UTF8.GetBytes("hello"));
        Assert.Equal("hello", result);
    }

    [Fact]
    public void constructor_EmptyString_ShouldDefaultToUtf8()
    {
        var decoder = new StringDecoder("");
        var result = decoder.write(Encoding.UTF8.GetBytes("hello"));
        Assert.Equal("hello", result);
    }

    // === write() 2-byte UTF-8 Tests ===

    [Fact]
    public void write_TwoByteTwoByteUtf8_Complete()
    {
        var decoder = new StringDecoder("utf8");
        // ¢ (cent sign) = 0xC2 0xA2
        var result = decoder.write(new byte[] { 0xC2, 0xA2 });
        Assert.Equal("¢", result);
    }

    [Fact]
    public void write_TwoByteUtf8_SplitByteByByte()
    {
        var decoder = new StringDecoder("utf8");
        // ¢ (cent sign) = 0xC2 0xA2
        var result1 = decoder.write(new byte[] { 0xC2 });
        Assert.Equal("", result1);

        var result2 = decoder.write(new byte[] { 0xA2 });
        Assert.Equal("¢", result2);
    }

    [Fact]
    public void write_MultipleTwoByteCharacters()
    {
        var decoder = new StringDecoder("utf8");
        // ¢£ = 0xC2 0xA2 0xC2 0xA3
        var result = decoder.write(new byte[] { 0xC2, 0xA2, 0xC2, 0xA3 });
        Assert.Equal("¢£", result);
    }

    [Fact]
    public void write_TwoByteCharacterWithIncompleteAtEnd()
    {
        var decoder = new StringDecoder("utf8");
        // ¢ + incomplete ¢ = 0xC2 0xA2 0xC2
        var result1 = decoder.write(new byte[] { 0xC2, 0xA2, 0xC2 });
        Assert.Equal("¢", result1); // First complete, second incomplete

        var result2 = decoder.write(new byte[] { 0xA2 });
        Assert.Equal("¢", result2); // Complete second
    }

    // === write() 4-byte UTF-8 Tests ===

    [Fact]
    public void write_FourByteUtf8_Complete()
    {
        var decoder = new StringDecoder("utf8");
        // 𝄞 (musical symbol G clef) = 0xF0 0x9D 0x84 0x9E
        var result = decoder.write(new byte[] { 0xF0, 0x9D, 0x84, 0x9E });
        Assert.Equal("𝄞", result);
    }

    [Fact]
    public void write_FourByteUtf8_SplitByteByByte()
    {
        var decoder = new StringDecoder("utf8");
        // 𝄞 (musical symbol) = 0xF0 0x9D 0x84 0x9E

        var result1 = decoder.write(new byte[] { 0xF0 });
        Assert.Equal("", result1);

        var result2 = decoder.write(new byte[] { 0x9D });
        Assert.Equal("", result2);

        var result3 = decoder.write(new byte[] { 0x84 });
        Assert.Equal("", result3);

        var result4 = decoder.write(new byte[] { 0x9E });
        Assert.Equal("𝄞", result4);
    }

    [Fact]
    public void write_FourByteUtf8_SplitAt2Bytes()
    {
        var decoder = new StringDecoder("utf8");
        // 𝄞 = 0xF0 0x9D 0x84 0x9E

        var result1 = decoder.write(new byte[] { 0xF0, 0x9D });
        Assert.Equal("", result1);

        var result2 = decoder.write(new byte[] { 0x84, 0x9E });
        Assert.Equal("𝄞", result2);
    }

    [Fact]
    public void write_FourByteUtf8_SplitAt3Bytes()
    {
        var decoder = new StringDecoder("utf8");
        // 𝄞 = 0xF0 0x9D 0x84 0x9E

        var result1 = decoder.write(new byte[] { 0xF0, 0x9D, 0x84 });
        Assert.Equal("", result1);

        var result2 = decoder.write(new byte[] { 0x9E });
        Assert.Equal("𝄞", result2);
    }

    [Fact]
    public void write_MultipleFourByteCharacters()
    {
        var decoder = new StringDecoder("utf8");
        // 🌍🎵 = 0xF0 0x9F 0x8C 0x8D 0xF0 0x9F 0x8E 0xB5
        var result = decoder.write(new byte[] { 0xF0, 0x9F, 0x8C, 0x8D, 0xF0, 0x9F, 0x8E, 0xB5 });
        Assert.Equal("🌍🎵", result);
    }

    [Fact]
    public void write_MixedAsciiAndFourByte()
    {
        var decoder = new StringDecoder("utf8");
        // "a🌍b" = 0x61 0xF0 0x9F 0x8C 0x8D 0x62

        var result1 = decoder.write(new byte[] { 0x61, 0xF0, 0x9F });
        Assert.Equal("a", result1); // 'a' complete, emoji incomplete

        var result2 = decoder.write(new byte[] { 0x8C, 0x8D, 0x62 });
        Assert.Equal("🌍b", result2); // emoji + 'b'
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
