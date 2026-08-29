using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class Zlib_gzipSyncTests
{
    [Fact]
    public void gzipSync_ShouldCompressData()
    {
        var data = Buffer.from("Hello, World!");
        var compressed = zlib.gzipSync(data);

        Assert.NotNull(compressed);
        Assert.True(compressed.length > 0);
        Assert.NotEqual(data.length, compressed.length);
    }

    [Fact]
    public void gzipSync_ShouldHaveGzipMagicBytes()
    {
        var data = Buffer.from("Test data");
        var compressed = zlib.gzipSync(data);

        // Gzip files start with 0x1f 0x8b
        Assert.Equal(0x1f, compressed[0]);
        Assert.Equal(0x8b, compressed[1]);
    }

    [Fact]
    public void gzipSync_WithCompressionLevel_ShouldWork()
    {
        var data = Buffer.from("Test data for compression");

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
        var data = Buffer.alloc(0);
        var compressed = zlib.gzipSync(data);

        Assert.NotNull(compressed);
        // Empty gzip has no content, just minimal headers
        Assert.True(compressed.length > 0);
    }

    [Fact]
    public void gzipSync_LargeData_ShouldCompress()
    {
        var data = Buffer.alloc(100000);
        for (int i = 0; i < data.length; i++)
        {
            data[i] = (byte)(i % 256);
        }

        var compressed = zlib.gzipSync(data);

        Assert.NotNull(compressed);
        Assert.True(compressed.length < data.length); // Should compress well
    }
}
