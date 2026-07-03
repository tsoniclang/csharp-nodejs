using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class Zlib_inflateSyncTests
{
    [Fact]
    public void inflateSync_ShouldDecompressData()
    {
        var original = Encoding.UTF8.GetBytes("Hello, World!");
        var compressed = zlib.deflateSync(original);
        var decompressed = zlib.inflateSync(compressed);

        Assert.Equal(original, decompressed);
    }

    [Fact]
    public void inflateSync_ShouldRestoreOriginalText()
    {
        var originalText = "The quick brown fox jumps over the lazy dog";
        var original = Encoding.UTF8.GetBytes(originalText);

        var compressed = zlib.deflateSync(original);
        var decompressed = zlib.inflateSync(compressed);
        var resultText = Encoding.UTF8.GetString(decompressed);

        Assert.Equal(originalText, resultText);
    }

    [Fact]
    public void inflateSync_WithNullBuffer_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => zlib.inflateSync(null!));
    }

    [Fact]
    public void inflateSync_WithInvalidData_ShouldThrow()
    {
        var invalidData = Encoding.UTF8.GetBytes("This is not compressed");

        Assert.Throws<System.IO.InvalidDataException>(() => zlib.inflateSync(invalidData));
    }
}
