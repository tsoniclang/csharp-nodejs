using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class Zlib_deflateSyncTests
{
    [Fact]
    public void deflateSync_ShouldCompressData()
    {
        var data = Buffer.from("Hello, World!");
        var compressed = zlib.deflateSync(data);

        Assert.NotNull(compressed);
        Assert.True(compressed.length > 0);
    }

    [Fact]
    public void deflateSync_WithCompressionLevel_ShouldWork()
    {
        var data = Buffer.from("Test data for compression");

        var compressed1 = zlib.deflateSync(data, new ZlibOptions { level = 1 });
        var compressed9 = zlib.deflateSync(data, new ZlibOptions { level = 9 });

        Assert.NotNull(compressed1);
        Assert.NotNull(compressed9);
    }

    [Fact]
    public void deflateSync_WithNullBuffer_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => zlib.deflateSync(null!));
    }

    [Fact]
    public void deflateSync_EmptyBuffer_ShouldCompress()
    {
        var data = Buffer.alloc(0);
        var compressed = zlib.deflateSync(data);

        Assert.NotNull(compressed);
        // Empty deflate produces minimal output
        Assert.True(compressed.length > 0);
    }
}
