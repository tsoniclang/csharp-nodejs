using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class Zlib_brotliCompressSyncTests
{
    [Fact]
    public void brotliCompressSync_ShouldCompressData()
    {
        var data = Encoding.UTF8.GetBytes("Hello, World!");
        var compressed = zlib.brotliCompressSync(data);

        Assert.NotNull(compressed);
        Assert.True(compressed.Length > 0);
    }

    [Fact]
    public void brotliCompressSync_WithQuality_ShouldWork()
    {
        var data = Encoding.UTF8.GetBytes("Test data for compression");

        var compressed1 = zlib.brotliCompressSync(data, new BrotliOptions { quality = 1 });
        var compressed11 = zlib.brotliCompressSync(data, new BrotliOptions { quality = 11 });

        Assert.NotNull(compressed1);
        Assert.NotNull(compressed11);
    }

    [Fact]
    public void brotliCompressSync_WithNullBuffer_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => zlib.brotliCompressSync(null!));
    }

    [Fact]
    public void brotliCompressSync_EmptyBuffer_ShouldCompress()
    {
        var data = Array.Empty<byte>();
        var compressed = zlib.brotliCompressSync(data);

        Assert.NotNull(compressed);
    }

    [Fact]
    public void brotliCompressSync_LargeData_ShouldCompress()
    {
        var data = new byte[50000];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(i % 256);
        }

        var compressed = zlib.brotliCompressSync(data);

        Assert.NotNull(compressed);
        Assert.True(compressed.Length < data.Length);
    }
}
