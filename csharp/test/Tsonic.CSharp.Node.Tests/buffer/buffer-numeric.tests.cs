using Xunit;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node.Tests;

public partial class BufferTests
{
    [Fact]
    public void readUInt8_ShouldReadByte()
    {
        var buf = Buffer.from(new[] { 42, 100 });
        Assert.Equal(42, buf.readUInt8(0));
        Assert.Equal(100, buf.readUInt8(1));
    }

    [Fact]
    public void readInt8_ShouldReadSignedByte()
    {
        var buf = Buffer.from(new[] { 127, 255 });
        Assert.Equal(127, buf.readInt8(0));
        Assert.Equal(-1, buf.readInt8(1));
    }

    [Fact]
    public void readUInt16LE_ShouldReadLittleEndian()
    {
        var buf = Buffer.from(new byte[] { 0x12, 0x34 });
        Assert.Equal(0x3412, buf.readUInt16LE(0));
    }

    [Fact]
    public void readUInt16BE_ShouldReadBigEndian()
    {
        var buf = Buffer.from(new byte[] { 0x12, 0x34 });
        Assert.Equal(0x1234, buf.readUInt16BE(0));
    }

    [Fact]
    public void readInt32LE_ShouldReadSignedIntLittleEndian()
    {
        var buf = Buffer.from(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
        Assert.Equal(-1, buf.readInt32LE(0));
    }

    [Fact]
    public void readFloatLE_ShouldReadFloat()
    {
        var buf = Buffer.alloc(4);
        buf.writeFloatLE(3.14f, 0);

        Assert.Equal(3.14f, buf.readFloatLE(0), 5);
    }

    [Fact]
    public void readDoubleLE_ShouldReadDouble()
    {
        var buf = Buffer.alloc(8);
        buf.writeDoubleLE(3.141592653589793, 0);

        Assert.Equal(3.141592653589793, buf.readDoubleLE(0), 15);
    }

    [Fact]
    public void writeUInt8_ShouldWriteByte()
    {
        var buf = Buffer.alloc(2);
        buf.writeUInt8(42, 0);
        buf.writeUInt8(100, 1);

        Assert.Equal(42, buf[0]);
        Assert.Equal(100, buf[1]);
    }

    [Fact]
    public void writeInt16LE_ShouldWriteSignedShortLittleEndian()
    {
        var buf = Buffer.alloc(2);
        buf.writeInt16LE(-1, 0);

        Assert.Equal(0xFF, buf[0]);
        Assert.Equal(0xFF, buf[1]);
    }

    [Fact]
    public void writeUInt32BE_ShouldWriteUnsignedIntBigEndian()
    {
        var buf = Buffer.alloc(4);
        buf.writeUInt32BE(0x12345678, 0);

        Assert.Equal(0x12, buf[0]);
        Assert.Equal(0x34, buf[1]);
        Assert.Equal(0x56, buf[2]);
        Assert.Equal(0x78, buf[3]);
    }

    [Fact]
    public void readWriteIntLE_VariableLength_ShouldWork()
    {
        var buf = Buffer.alloc(6);
        buf.writeIntLE(0x123456, 0, 3);

        Assert.Equal(0x123456, buf.readIntLE(0, 3));
    }

    [Fact]
    public void readWriteUIntBE_VariableLength_ShouldWork()
    {
        var buf = Buffer.alloc(6);
        buf.writeUIntBE(0x123456, 0, 3);

        Assert.Equal((ulong)0x123456, buf.readUIntBE(0, 3));
    }
}
