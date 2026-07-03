using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection("PerfHooks")]
public class PerfHooks_ExtendedTests
{
    [Fact]
    public void ResourceTiming_ShouldRecordExposeJsonAndClear()
    {
        performance.clearResourceTimings();
        var entry = performance.addResourceTiming("https://example.com/app.js", 10, 20, "cache");
        var json = entry.toJSON();

        Assert.Equal("resource", entry.entryType);
        Assert.Equal("cache", entry.deliveryType);
        Assert.Equal("cache", json["deliveryType"]);
        Assert.Contains(performance.getEntriesByType("resource"), item => item.name == entry.name);

        performance.clearResourceTimings();
        Assert.Empty(performance.getEntriesByType("resource"));
    }

    [Fact]
    public void Histogram_ShouldRecordPercentilesAndReset()
    {
        var histogram = performance.createHistogram();

        histogram.record(10);
        histogram.record(20);
        histogram.record(30);

        Assert.Equal(3, histogram.count);
        Assert.Equal(10, histogram.min);
        Assert.Equal(30, histogram.max);
        Assert.Equal(20, histogram.percentile(50));
        Assert.Equal(30, histogram.percentileBigInt(100));
        Assert.Contains(99, histogram.percentiles().Keys);

        histogram.reset();
        Assert.Equal(0, histogram.countBigInt);
    }

    [Fact]
    public void EventLoopUtilizationAndConstants_ShouldBeAvailable()
    {
        var first = performance.eventLoopUtilization();
        var second = performance.eventLoopUtilization(first);

        Assert.True(first.active >= 0);
        Assert.True(second.active >= 0);
        Assert.True(performance.constants.ContainsKey("NODE_PERFORMANCE_GC_MAJOR"));
        Assert.Equal("node", performance.nodeTiming.name);
    }

    [Fact]
    public void Timerify_ShouldRecordFunctionDuration()
    {
        var histogram = performance.createHistogram();
        var wrapped = performance.timerify(() => 42, histogram);

        Assert.Equal(42, wrapped());
        Assert.Equal(1, histogram.count);
    }
}
