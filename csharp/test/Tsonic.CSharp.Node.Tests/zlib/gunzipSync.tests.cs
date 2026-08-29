using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class Zlib_gunzipSyncTests
{
    [Fact]
    public void gunzipSync_ShouldDecompressData()
    {
        var original = Buffer.from("Hello, World!");
        var compressed = zlib.gzipSync(original);
        var decompressed = zlib.gunzipSync(compressed);

        Assert.Equal(original.toString(), decompressed.toString());
    }

    [Fact]
    public void gunzipSync_ShouldRestoreOriginalText()
    {
        var originalText = "The quick brown fox jumps over the lazy dog";
        var original = Buffer.from(originalText);

        var compressed = zlib.gzipSync(original);
        var decompressed = zlib.gunzipSync(compressed);
        var resultText = decompressed.toString();

        Assert.Equal(originalText, resultText);
    }

    [Fact]
    public void gunzipSync_WithNullBuffer_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => zlib.gunzipSync(null!));
    }

    [Fact]
    public void gunzipSync_WithInvalidData_ShouldThrow()
    {
        var invalidData = Buffer.from("This is not compressed");

        Assert.Throws<System.IO.InvalidDataException>(() => zlib.gunzipSync(invalidData));
    }

    [Fact]
    public void gunzipSync_EmptyCompressedData_ShouldDecompress()
    {
        var empty = Buffer.alloc(0);
        var compressed = zlib.gzipSync(empty);
        var decompressed = zlib.gunzipSync(compressed);

        Assert.Equal(0, decompressed.length);
    }

    [Fact]
    public void gunzipSync_LargeData_ShouldDecompress()
    {
        var original = Buffer.alloc(100000);
        for (int i = 0; i < original.length; i++)
        {
            original[i] = (byte)(i % 256);
        }

        var compressed = zlib.gzipSync(original);
        var decompressed = zlib.gunzipSync(compressed);

        Assert.Equal(original.length, decompressed.length);
        for (var index = 0; index < original.length; index++)
            Assert.Equal(original[index], decompressed[index]);
    }
}
