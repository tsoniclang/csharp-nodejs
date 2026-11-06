using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class NativeSurfaceNumericContractsTests
{
    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void Int32BufferValuesRetainTheirExactNativeRange(int value)
    {
        var buffer = Buffer.alloc(4);
        int written = buffer.writeInt32LE(value);
        int actual = buffer.readInt32LE();
        Assert.Equal(4, written);
        Assert.Equal(value, actual);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(2147483648u)]
    [InlineData(uint.MaxValue)]
    public void UInt32BufferValuesRetainTheirUnsignedNativeRange(uint value)
    {
        var buffer = Buffer.alloc(4);
        buffer.writeUInt32LE(value);
        uint actual = buffer.readUInt32LE();
        Assert.Equal(value, actual);
    }

    [Theory]
    [InlineData(9007199254740993L)]
    [InlineData(long.MaxValue)]
    public void FileAndProcessMetricsRetainInt64BeyondDoublePrecision(long value)
    {
        var stats = new Stats { size = value };
        var memory = new MemoryUsage { rss = value, arrayBuffers = value };
        var cpu = new CpuUsage { user = value, system = value };
        Assert.Equal(value, stats.size);
        Assert.Equal(value, memory.rss);
        Assert.Equal(value, memory.arrayBuffers);
        Assert.Equal(value, cpu.user);
        Assert.Equal(value, cpu.system);
    }

    [Fact]
    public void DnsFamilyHasOneNullableInt32NativeContract()
    {
        Assert.Equal(typeof(int?), typeof(LookupOptions).GetProperty(nameof(LookupOptions.family))!.PropertyType);
        Assert.Null(new LookupOptions().family);
        foreach (int family in new[] { 0, 4, 6 })
        {
            int? actual = new LookupOptions { family = family }.family;
            Assert.Equal(family, actual);
        }
    }

    [Fact]
    public void SupportedBufferDescriptorCallsRetainTheirPublishedOmissionContract()
    {
        foreach (string name in new[] { "readSync", "writeSync" })
        {
            var member = typeof(fs).GetMethod(name, new[]
            {
                typeof(int), typeof(Buffer), typeof(int), typeof(int), typeof(long?)
            }) ?? throw new InvalidOperationException("Missing exact Buffer descriptor operation.");
            var position = member.GetParameters()[4];
            Assert.True(position.HasDefaultValue);
            Assert.Null(position.DefaultValue);
        }
    }

    [Fact]
    public void FloatingIntegerCoercionIsNotAnExportedNodeRuntimeApi()
    {
        Assert.Null(typeof(JsNumeric).GetMethod("RequireInteger"));
    }
}
