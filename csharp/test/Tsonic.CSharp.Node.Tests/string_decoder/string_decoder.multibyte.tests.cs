using System.Text;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public partial class StringDecoderTests
{

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
}
