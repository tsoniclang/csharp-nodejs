using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class Zlib_brotliDecompressSyncTests
{
    [Fact]
    public void brotliDecompressSync_ShouldDecompressData()
    {
        var original = Buffer.from("Hello, World!");
        var compressed = zlib.brotliCompressSync(original);
        var decompressed = zlib.brotliDecompressSync(compressed);

        Assert.Equal(original.toString(), decompressed.toString());
    }

    [Fact]
    public void brotliDecompressSync_ShouldRestoreOriginalText()
    {
        var originalText = "The quick brown fox jumps over the lazy dog";
        var original = Buffer.from(originalText);

        var compressed = zlib.brotliCompressSync(original);
        var decompressed = zlib.brotliDecompressSync(compressed);
        var resultText = decompressed.toString();

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
        var invalidData = Buffer.from("This is not compressed");

        Assert.Throws<InvalidOperationException>(() => zlib.brotliDecompressSync(invalidData));
    }

    [Fact]
    public void brotliDecompressSync_LargeData_ShouldDecompress()
    {
        var original = Buffer.alloc(50000);
        for (int i = 0; i < original.length; i++)
        {
            original[i] = (byte)(i % 256);
        }

        var compressed = zlib.brotliCompressSync(original);
        var decompressed = zlib.brotliDecompressSync(compressed);

        Assert.Equal(original.length, decompressed.length);
        for (var index = 0; index < original.length; index++)
            Assert.Equal(original[index], decompressed[index]);
    }
}
