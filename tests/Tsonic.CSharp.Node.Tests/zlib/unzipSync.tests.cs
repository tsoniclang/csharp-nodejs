using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class Zlib_unzipSyncTests
{
    [Fact]
    public void unzipSync_ShouldDecompressGzip()
    {
        var original = Encoding.UTF8.GetBytes("Hello, World!");
        var compressed = zlib.gzipSync(original);
        var decompressed = zlib.unzipSync(compressed);

        Assert.Equal(original, decompressed);
    }

    [Fact]
    public void unzipSync_ShouldDecompressDeflate()
    {
        var original = Encoding.UTF8.GetBytes("Hello, World!");
        var compressed = zlib.deflateSync(original);
        var decompressed = zlib.unzipSync(compressed);

        Assert.Equal(original, decompressed);
    }

    [Fact]
    public void unzipSync_WithGzipData_ShouldAutoDetect()
    {
        var originalText = "Test data for auto-detection";
        var original = Encoding.UTF8.GetBytes(originalText);
        var compressed = zlib.gzipSync(original);
        var decompressed = zlib.unzipSync(compressed);
        var resultText = Encoding.UTF8.GetString(decompressed);

        Assert.Equal(originalText, resultText);
    }

    [Fact]
    public void unzipSync_WithNullBuffer_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => zlib.unzipSync(null!));
    }

    [Fact]
    public void unzipSync_WithTooSmallBuffer_ShouldThrow()
    {
        var tooSmall = new byte[] { 0x00 };

        Assert.Throws<ArgumentException>(() => zlib.unzipSync(tooSmall));
    }
}
