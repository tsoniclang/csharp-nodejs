using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class HeapInfoNativeWidthsTests
{
    [Theory]
    [InlineData("total_heap_size")]
    [InlineData("total_heap_size_executable")]
    [InlineData("total_physical_size")]
    [InlineData("total_available_size")]
    [InlineData("used_heap_size")]
    [InlineData("heap_size_limit")]
    [InlineData("malloced_memory")]
    [InlineData("peak_malloced_memory")]
    [InlineData("total_global_handles_size")]
    [InlineData("used_global_handles_size")]
    [InlineData("external_memory")]
    [InlineData("total_allocated_bytes")]
    public void ByteCountsRetainExactNativeInt64Storage(string memberName)
    {
        var property = typeof(HeapInfo).GetProperty(memberName);
        Assert.NotNull(property);
        Assert.Equal(typeof(long), property.PropertyType);
        var value = new HeapInfo();
        foreach (var count in new[] { 0L, 9007199254740993L, long.MinValue, long.MaxValue })
        {
            property.SetValue(value, count);
            Assert.Equal(count, Assert.IsType<long>(property.GetValue(value)));
        }
    }
}
