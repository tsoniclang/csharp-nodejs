using Xunit;
using System;
using System.Threading;

namespace Tsonic.CSharp.Node.Tests;

[Collection("PerfHooks")]
public class PerfHooks_performanceTests
{
    [Fact]
    public void performance_now_ShouldReturnPositiveNumber()
    {
        var timestamp = performance.now();

        Assert.True(timestamp > 0);
    }

    [Fact]
    public void performance_now_ShouldBeMonotonicIncreasing()
    {
        var t1 = performance.now();
        Thread.Sleep(10); // Sleep for 10ms
        var t2 = performance.now();

        Assert.True(t2 > t1);
        Assert.True(t2 - t1 >= 10); // At least 10ms should have passed
    }

    [Fact]
    public void performance_mark_ShouldCreateMark()
    {
        performance.clearMarks(); // Clean slate

        var mark = performance.mark("test-mark");

        Assert.NotNull(mark);
        Assert.Equal("test-mark", mark.name);
        Assert.Equal("mark", mark.entryType);
        Assert.Equal(0, mark.duration);
        Assert.True(mark.startTime > 0);
    }

    [Fact]
    public void performance_mark_WithNullName_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => performance.mark(null!));
    }

    [Fact]
    public void performance_mark_WithEmptyName_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => performance.mark(""));
    }

    [Fact]
    public void performance_mark_WithDetail_ShouldStoreDetail()
    {
        performance.clearMarks();

        var detail = new { foo = "bar", count = 42 };
        var mark = performance.mark("detailed-mark", new MarkOptions { detail = detail });

        Assert.Equal(detail, mark.detail);
    }

    [Fact]
    public void performance_mark_WithCustomStartTime_ShouldUseProvidedTime()
    {
        performance.clearMarks();

        var customTime = 12345.678;
        var mark = performance.mark("custom-time-mark", new MarkOptions { startTime = customTime });

        Assert.Equal(customTime, mark.startTime);
    }

    [Fact]
    public void performance_measure_BetweenMarks_ShouldCalculateDuration()
    {
        performance.clearMarks();
        performance.clearMeasures();

        performance.mark("start");
        Thread.Sleep(50); // Sleep for 50ms
        performance.mark("end");

        var measure = performance.measure("test-measure", "start", "end");

        Assert.NotNull(measure);
        Assert.Equal("test-measure", measure.name);
        Assert.Equal("measure", measure.entryType);
        Assert.True(measure.duration >= 50); // At least 50ms
        Assert.True(measure.duration < 100); // But less than 100ms (reasonable upper bound)
    }

    [Fact]
    public void performance_measure_WithNullName_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => performance.measure(null!));
    }

    [Fact]
    public void performance_measure_WithEmptyName_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => performance.measure(""));
    }

    [Fact]
    public void performance_measure_WithMissingStartMark_ShouldUseZero()
    {
        performance.clearMarks();
        performance.clearMeasures();

        performance.mark("end");
        var measure = performance.measure("test", "nonexistent-start", "end");

        Assert.Equal(0, measure.startTime);
    }

    [Fact]
    public void performance_measure_WithMissingEndMark_ShouldUseNow()
    {
        performance.clearMarks();
        performance.clearMeasures();

        var startMark = performance.mark("start");
        Thread.Sleep(10);
        var beforeMeasure = performance.now();
        var measure = performance.measure("test", "start", "nonexistent-end");

        Assert.True(measure.startTime == startMark.startTime);
        Assert.True(measure.duration > 10); // Should be at least 10ms
        Assert.True(measure.startTime + measure.duration <= beforeMeasure + 5); // Reasonable tolerance
    }

    [Fact]
    public void performance_measure_WithNoMarks_ShouldWorkWithCurrentTime()
    {
        performance.clearMarks();
        performance.clearMeasures();

        var measure = performance.measure("test-no-marks");

        Assert.NotNull(measure);
        Assert.Equal(0, measure.startTime);
        Assert.True(measure.duration > 0);
    }

    [Fact]
    public void performance_measure_WithOptions_ShouldWork()
    {
        performance.clearMarks();
        performance.clearMeasures();

        performance.mark("start");
        Thread.Sleep(50);
        performance.mark("end");

        var detail = new { type = "test" };
        var measure = performance.measure("test-options", new MeasureOptions
        {
            startMark = "start",
            endMark = "end",
            detail = detail
        });

        Assert.Equal(detail, measure.detail);
        Assert.True(measure.duration >= 50);
    }

    [Fact]
    public void performance_measure_WithExplicitTimes_ShouldWork()
    {
        performance.clearMeasures();

        var measure = performance.measure("explicit-times", new MeasureOptions
        {
            start = 100.0,
            end = 250.5
        });

        Assert.Equal(100.0, measure.startTime);
        Assert.Equal(150.5, measure.duration);
    }

    [Fact]
    public void performance_getEntries_ShouldReturnAllEntries()
    {
        performance.clearMarks();
        performance.clearMeasures();

        performance.mark("mark1");
        performance.mark("mark2");
        performance.measure("measure1");

        var entries = performance.getEntries();

        Assert.True(entries.Length >= 3);
        Assert.Contains(entries, e => e.name == "mark1");
        Assert.Contains(entries, e => e.name == "mark2");
        Assert.Contains(entries, e => e.name == "measure1");
    }

    [Fact]
    public void performance_getEntriesByName_ShouldFilterByName()
    {
        performance.clearMarks();
        performance.clearMeasures();

        performance.mark("test");
        performance.mark("other");
        performance.measure("test");

        var entries = performance.getEntriesByName("test");

        Assert.Equal(2, entries.Length);
        Assert.All(entries, e => Assert.Equal("test", e.name));
    }

    [Fact]
    public void performance_getEntriesByName_WithType_ShouldFilterByNameAndType()
    {
        performance.clearMarks();
        performance.clearMeasures();

        performance.mark("test");
        performance.measure("test");

        var entries = performance.getEntriesByName("test", "mark");

        Assert.Single(entries);
        Assert.Equal("test", entries[0].name);
        Assert.Equal("mark", entries[0].entryType);
    }

    [Fact]
    public void performance_getEntriesByName_WithNullName_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => performance.getEntriesByName(null!));
    }

    [Fact]
    public void performance_getEntriesByType_ShouldFilterByType()
    {
        performance.clearMarks();
        performance.clearMeasures();

        performance.mark("mark1");
        performance.mark("mark2");
        performance.measure("measure1");

        var marks = performance.getEntriesByType("mark");
        var measures = performance.getEntriesByType("measure");

        Assert.True(marks.Length >= 2);
        Assert.All(marks, e => Assert.Equal("mark", e.entryType));
        Assert.True(measures.Length >= 1);
        Assert.All(measures, e => Assert.Equal("measure", e.entryType));
    }

    [Fact]
    public void performance_getEntriesByType_WithNullType_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => performance.getEntriesByType(null!));
    }

    [Fact]
    public void performance_clearMarks_ShouldRemoveAllMarks()
    {
        performance.clearMarks();

        performance.mark("mark1");
        performance.mark("mark2");

        performance.clearMarks();

        var marks = performance.getEntriesByType("mark");
        Assert.Empty(marks);
    }

    [Fact]
    public void performance_clearMarks_WithName_ShouldRemoveSpecificMark()
    {
        performance.clearMarks();

        performance.mark("keep");
        performance.mark("remove");

        performance.clearMarks("remove");

        var marks = performance.getEntriesByType("mark");
        Assert.Single(marks);
        Assert.Equal("keep", marks[0].name);
    }

    [Fact]
    public void performance_clearMeasures_ShouldRemoveAllMeasures()
    {
        performance.clearMeasures();

        performance.measure("measure1");
        performance.measure("measure2");

        performance.clearMeasures();

        var measures = performance.getEntriesByType("measure");
        Assert.Empty(measures);
    }

    [Fact]
    public void performance_clearMeasures_WithName_ShouldRemoveSpecificMeasure()
    {
        performance.clearMeasures();

        performance.measure("keep");
        performance.measure("remove");

        performance.clearMeasures("remove");

        var measures = performance.getEntriesByType("measure");
        Assert.Single(measures);
        Assert.Equal("keep", measures[0].name);
    }
}
