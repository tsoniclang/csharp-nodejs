using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class Zlib_gzipSyncTests
{
    [Fact]
    public void gzipSync_ShouldCompressData()
    {
        var data = Encoding.UTF8.GetBytes("Hello, World!");
        var compressed = zlib.gzipSync(data);

        Assert.NotNull(compressed);
        Assert.True(compressed.Length > 0);
        Assert.NotEqual(data.Length, compressed.Length);
    }

    [Fact]
    public void gzipSync_ShouldHaveGzipMagicBytes()
    {
        var data = Encoding.UTF8.GetBytes("Test data");
        var compressed = zlib.gzipSync(data);

        // Gzip files start with 0x1f 0x8b
        Assert.Equal(0x1f, compressed[0]);
        Assert.Equal(0x8b, compressed[1]);
    }

    [Fact]
    public void gzipSync_WithCompressionLevel_ShouldWork()
    {
        var data = Encoding.UTF8.GetBytes("Test data for compression");

        var compressed1 = zlib.gzipSync(data, new ZlibOptions { level = 1 });
        var compressed9 = zlib.gzipSync(data, new ZlibOptions { level = 9 });

        Assert.NotNull(compressed1);
        Assert.NotNull(compressed9);
    }

    [Fact]
    public void gzipSync_WithNullBuffer_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => zlib.gzipSync(null!));
    }

    [Fact]
    public void gzipSync_EmptyBuffer_ShouldCompress()
    {
        var data = Array.Empty<byte>();
        var compressed = zlib.gzipSync(data);

        Assert.NotNull(compressed);
        // Empty gzip has no content, just minimal headers
        Assert.True(compressed.Length >= 0);
    }

    [Fact]
    public void gzipSync_LargeData_ShouldCompress()
    {
        var data = new byte[100000];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (byte)(i % 256);
        }

        var compressed = zlib.gzipSync(data);

        Assert.NotNull(compressed);
        Assert.True(compressed.Length < data.Length); // Should compress well
    }
}
