using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class Zlib_gunzipSyncTests
{
    [Fact]
    public void gunzipSync_ShouldDecompressData()
    {
        var original = Encoding.UTF8.GetBytes("Hello, World!");
        var compressed = zlib.gzipSync(original);
        var decompressed = zlib.gunzipSync(compressed);

        Assert.Equal(original, decompressed);
    }

    [Fact]
    public void gunzipSync_ShouldRestoreOriginalText()
    {
        var originalText = "The quick brown fox jumps over the lazy dog";
        var original = Encoding.UTF8.GetBytes(originalText);

        var compressed = zlib.gzipSync(original);
        var decompressed = zlib.gunzipSync(compressed);
        var resultText = Encoding.UTF8.GetString(decompressed);

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
        var invalidData = Encoding.UTF8.GetBytes("This is not compressed");

        Assert.Throws<System.IO.InvalidDataException>(() => zlib.gunzipSync(invalidData));
    }

    [Fact]
    public void gunzipSync_EmptyCompressedData_ShouldDecompress()
    {
        var empty = Array.Empty<byte>();
        var compressed = zlib.gzipSync(empty);
        var decompressed = zlib.gunzipSync(compressed);

        Assert.Equal(empty, decompressed);
    }

    [Fact]
    public void gunzipSync_LargeData_ShouldDecompress()
    {
        var original = new byte[100000];
        for (int i = 0; i < original.Length; i++)
        {
            original[i] = (byte)(i % 256);
        }

        var compressed = zlib.gzipSync(original);
        var decompressed = zlib.gunzipSync(compressed);

        Assert.Equal(original, decompressed);
    }
}
