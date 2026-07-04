using System.Text;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public partial class StringDecoderTests
{

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
}
