using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class Zlib_crc32Tests
{
    [Fact]
    public void crc32_WithByteArray_ShouldReturnChecksum()
    {
        var data = Encoding.UTF8.GetBytes("Hello, World!");
        var checksum = zlib.crc32(data);

        Assert.True(checksum > 0);
    }

    [Fact]
    public void crc32_WithString_ShouldReturnChecksum()
    {
        var checksum = zlib.crc32("Hello, World!");

        Assert.True(checksum > 0);
    }

    [Fact]
    public void crc32_SameData_ShouldReturnSameChecksum()
    {
        var data = Encoding.UTF8.GetBytes("Test data");

        var checksum1 = zlib.crc32(data);
        var checksum2 = zlib.crc32(data);

        Assert.Equal(checksum1, checksum2);
    }

    [Fact]
    public void crc32_DifferentData_ShouldReturnDifferentChecksum()
    {
        var checksum1 = zlib.crc32("Data 1");
        var checksum2 = zlib.crc32("Data 2");

        Assert.NotEqual(checksum1, checksum2);
    }

    [Fact]
    public void crc32_EmptyData_ShouldReturnInitialValue()
    {
        var checksum = zlib.crc32(Array.Empty<byte>());

        // CRC32 of empty data returns the inverted initial value (0xFFFFFFFF)
        Assert.Equal(0u, checksum);
    }

    [Fact]
    public void crc32_WithNullByteArray_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => zlib.crc32((byte[])null!));
    }

    [Fact]
    public void crc32_WithNullString_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => zlib.crc32((string)null!));
    }
}
