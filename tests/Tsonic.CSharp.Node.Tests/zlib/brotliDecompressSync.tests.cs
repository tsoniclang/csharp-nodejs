using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class Zlib_brotliDecompressSyncTests
{
    [Fact]
    public void brotliDecompressSync_ShouldDecompressData()
    {
        var original = Encoding.UTF8.GetBytes("Hello, World!");
        var compressed = zlib.brotliCompressSync(original);
        var decompressed = zlib.brotliDecompressSync(compressed);

        Assert.Equal(original, decompressed);
    }

    [Fact]
    public void brotliDecompressSync_ShouldRestoreOriginalText()
    {
        var originalText = "The quick brown fox jumps over the lazy dog";
        var original = Encoding.UTF8.GetBytes(originalText);

        var compressed = zlib.brotliCompressSync(original);
        var decompressed = zlib.brotliDecompressSync(compressed);
        var resultText = Encoding.UTF8.GetString(decompressed);

        Assert.Equal(originalText, resultText);
    }

    [Fact]
    public void brotliDecompressSync_WithNullBuffer_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => zlib.brotliDecompressSync(null!));
    }

    [Fact]
    public void brotliDecompressSync_WithInvalidData_ShouldThrow()
    {
        var invalidData = Encoding.UTF8.GetBytes("This is not compressed");

        Assert.Throws<InvalidOperationException>(() => zlib.brotliDecompressSync(invalidData));
    }

    [Fact]
    public void brotliDecompressSync_LargeData_ShouldDecompress()
    {
        var original = new byte[50000];
        for (int i = 0; i < original.Length; i++)
        {
            original[i] = (byte)(i % 256);
        }

        var compressed = zlib.brotliCompressSync(original);
        var decompressed = zlib.brotliDecompressSync(compressed);

        Assert.Equal(original, decompressed);
    }
}
