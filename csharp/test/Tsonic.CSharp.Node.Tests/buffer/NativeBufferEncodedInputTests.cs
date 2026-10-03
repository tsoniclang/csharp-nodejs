using System;
using System.Text;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class NativeBufferEncodedInputTests
{
    [Theory]
    [InlineData("0080ff", "hex", "0080ff")]
    [InlineData("0080FF", "HEX", "0080ff")]
    [InlineData("aBcDeF", "HeX", "abcdef")]
    [InlineData("", "hex", "")]
    [InlineData("AID/", "base64", "0080ff")]
    [InlineData("AP8=", "BASE64", "00ff")]
    [InlineData("AA==", "BaSe64", "00")]
    [InlineData(" A\tP8=\r\n", "base64", "00ff")]
    [InlineData("", "base64", "")]
    [InlineData("AID_", "base64url", "0080ff")]
    [InlineData("-_8", "BASE64URL", "fbff")]
    [InlineData("AA", "Base64Url", "00")]
    [InlineData("AP8=", "base64url", "00ff")]
    [InlineData("", "base64url", "")]
    public void EncodedInputRetainsExactBinaryBytes(string value, string encoding, string expected)
    {
        Assert.True(Buffer.isEncoding(encoding));
        var buffer = Buffer.from(value, encoding);
        Assert.Equal(expected.Length / 2, buffer.length);
        Assert.Equal(expected, buffer.toString("hex"));
    }

    [Theory]
    [InlineData("é", "utf8", "c3a9")]
    [InlineData("é", "UTF-8", "c3a9")]
    [InlineData("ABC", "ascii", "414243")]
    [InlineData("ÿ", "latin1", "ff")]
    [InlineData("ÿ", "binary", "ff")]
    [InlineData("é", "utf16le", "e900")]
    [InlineData("é", "UTF-16LE", "e900")]
    [InlineData("é", "ucs2", "e900")]
    [InlineData("é", "UCS-2", "e900")]
    public void TextEncodingPathsKeepTheirNativeBytes(string value, string encoding, string expected)
    {
        Assert.True(Buffer.isEncoding(encoding));
        Assert.Equal(expected, Buffer.from(value, encoding).toString("hex"));
    }

    [Theory]
    [InlineData("0", "hex")]
    [InlineData("gg", "hex")]
    [InlineData("00gg", "hex")]
    [InlineData("00 ff", "hex")]
    [InlineData("A", "base64")]
    [InlineData("AP8", "base64")]
    [InlineData("!!!!", "base64")]
    [InlineData("AA=A", "base64")]
    [InlineData("A", "base64url")]
    [InlineData("!!!!", "base64url")]
    public void MalformedBinaryInputKeepsNativeDecodeRejection(string value, string encoding)
    {
        Assert.Throws<FormatException>(() => Buffer.from(value, encoding));
    }

    [Fact]
    public void UnknownEncodingIsRejectedAtTheEncodingBoundary()
    {
        var failure = Assert.Throws<ArgumentException>(() => Buffer.from("text", "unknown"));
        Assert.Equal("encoding", failure.ParamName);
    }

    [Fact]
    public void DecodedBuffersOwnIndependentStorageAndConcatExactBytes()
    {
        var first = Buffer.from("80ff", "hex");
        var independent = Buffer.from("80ff", "hex");
        first[0] = 1;
        Assert.Equal(128, independent[0]);
        var prefix = Buffer.from("AA==", "base64");
        var result = Buffer.concat(new[] { prefix, independent }, prefix.length + independent.length);
        Assert.Equal("0080ff", result.toString("hex"));
        independent[1] = 7;
        Assert.Equal("0080ff", result.toString("hex"));
    }

    [Theory]
    [InlineData("hex")]
    [InlineData("base64")]
    [InlineData("utf8")]
    public void DecodingAddsOnlyTheBufferOwnerToNativePayloadAllocation(string encoding)
    {
        var source = new byte[32 * 1024];
        for (var index = 0; index < source.Length; index++) source[index] = 97;
        var value = encoding switch
        {
            "hex" => Convert.ToHexString(source),
            "base64" => Convert.ToBase64String(source),
            _ => Encoding.UTF8.GetString(source)
        };
        Func<byte[]> decode = encoding switch
        {
            "hex" => () => Convert.FromHexString(value),
            "base64" => () => Convert.FromBase64String(value),
            _ => () => Encoding.UTF8.GetBytes(value)
        };
        for (var iteration = 0; iteration < 128; iteration++)
        {
            GC.KeepAlive(decode());
            GC.KeepAlive(Buffer.from(value, encoding));
        }
        const int repetitions = 64;
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var iteration = 0; iteration < repetitions; iteration++) GC.KeepAlive(decode());
        var nativeAllocated = GC.GetAllocatedBytesForCurrentThread() - before;
        before = GC.GetAllocatedBytesForCurrentThread();
        for (var iteration = 0; iteration < repetitions; iteration++) GC.KeepAlive(Buffer.from(value, encoding));
        var runtimeAllocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.InRange(runtimeAllocated - nativeAllocated, 0L, 64L * repetitions);
    }
}
