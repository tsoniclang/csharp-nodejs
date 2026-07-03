using Xunit;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Tsonic.CSharp.Node.Tests;

[Collection("PerfHooks")]
public class PerfHooks_PerformanceObserverTests
{
    [Fact]
    public void PerformanceObserver_WithNullCallback_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => new PerformanceObserver(null!));
    }

    [Fact]
    public void PerformanceObserver_observe_WithNullOptions_ShouldThrow()
    {
        var observer = new PerformanceObserver((list, obs) => { });

        Assert.Throws<ArgumentNullException>(() => observer.observe(null!));
    }

    [Fact]
    public void PerformanceObserver_observe_WithEmptyEntryTypes_ShouldThrow()
    {
        var observer = new PerformanceObserver((list, obs) => { });

        Assert.Throws<ArgumentException>(() => observer.observe(new PerformanceObserverOptions
        {
            entryTypes = Array.Empty<string>()
        }));
    }

    [Fact]
    public void PerformanceObserver_observe_WithNullEntryTypes_ShouldThrow()
    {
        var observer = new PerformanceObserver((list, obs) => { });

        Assert.Throws<ArgumentException>(() => observer.observe(new PerformanceObserverOptions()));
    }

    [Fact]
    public void PerformanceObserver_ShouldReceiveMarkEntries()
    {
        performance.clearMarks();

        var received = new List<PerformanceEntry>();
        var observer = new PerformanceObserver((list, obs) =>
        {
            received.AddRange(list.getEntries());
        });

        observer.observe(new PerformanceObserverOptions
        {
            entryTypes = new[] { "mark" }
        });

        performance.mark("observed-mark");

        // Give observer a moment to process
        Thread.Sleep(10);

        Assert.Single(received);
        Assert.Equal("observed-mark", received[0].name);
        Assert.Equal("mark", received[0].entryType);

        observer.disconnect();
    }

    [Fact]
    public void PerformanceObserver_ShouldReceiveMeasureEntries()
    {
        performance.clearMeasures();

        var received = new List<PerformanceEntry>();
        var observer = new PerformanceObserver((list, obs) =>
        {
            received.AddRange(list.getEntries());
        });

        observer.observe(new PerformanceObserverOptions
        {
            entryTypes = new[] { "measure" }
        });

        performance.measure("observed-measure");

        // Give observer a moment to process
        Thread.Sleep(10);

        Assert.Single(received);
        Assert.Equal("observed-measure", received[0].name);
        Assert.Equal("measure", received[0].entryType);

        observer.disconnect();
    }

    [Fact]
    public void PerformanceObserver_ShouldReceiveMultipleEntryTypes()
    {
        performance.clearMarks();
        performance.clearMeasures();

        var received = new List<PerformanceEntry>();
        var observer = new PerformanceObserver((list, obs) =>
        {
            received.AddRange(list.getEntries());
        });

        observer.observe(new PerformanceObserverOptions
        {
            entryTypes = new[] { "mark", "measure" }
        });

        performance.mark("test-mark");
        performance.measure("test-measure");

        // Give observer a moment to process
        Thread.Sleep(10);

        Assert.Equal(2, received.Count);
        Assert.Contains(received, e => e.name == "test-mark" && e.entryType == "mark");
        Assert.Contains(received, e => e.name == "test-measure" && e.entryType == "measure");

        observer.disconnect();
    }

    [Fact]
    public void PerformanceObserver_ShouldNotReceiveAfterDisconnect()
    {
        performance.clearMarks();

        var received = new List<PerformanceEntry>();
        var observer = new PerformanceObserver((list, obs) =>
        {
            received.AddRange(list.getEntries());
        });

        observer.observe(new PerformanceObserverOptions
        {
            entryTypes = new[] { "mark" }
        });

        performance.mark("before-disconnect");
        Thread.Sleep(10);

        observer.disconnect();

        performance.mark("after-disconnect");
        Thread.Sleep(10);

        Assert.Single(received);
        Assert.Equal("before-disconnect", received[0].name);
    }

    [Fact]
    public void PerformanceObserver_ShouldFilterByEntryType()
    {
        performance.clearMarks();
        performance.clearMeasures();

        var received = new List<PerformanceEntry>();
        var observer = new PerformanceObserver((list, obs) =>
        {
            received.AddRange(list.getEntries());
        });

        // Only observe marks
        observer.observe(new PerformanceObserverOptions
        {
            entryTypes = new[] { "mark" }
        });

        performance.mark("should-receive");
        performance.measure("should-not-receive");

        Thread.Sleep(10);

        Assert.Single(received);
        Assert.Equal("should-receive", received[0].name);

        observer.disconnect();
    }

    [Fact]
    public void PerformanceObserver_takeRecords_ShouldReturnEmptyList()
    {
        var observer = new PerformanceObserver((list, obs) => { });

        observer.observe(new PerformanceObserverOptions
        {
            entryTypes = new[] { "mark" }
        });

        var records = observer.takeRecords();

        Assert.NotNull(records);
        Assert.Empty(records.getEntries());

        observer.disconnect();
    }

    [Fact]
    public void PerformanceObserver_CallbackShouldReceiveSelf()
    {
        performance.clearMarks();

        PerformanceObserver? receivedObserver = null;
        var observer = new PerformanceObserver((list, obs) =>
        {
            receivedObserver = obs;
        });

        observer.observe(new PerformanceObserverOptions
        {
            entryTypes = new[] { "mark" }
        });

        performance.mark("test");
        Thread.Sleep(10);

        Assert.Same(observer, receivedObserver);

        observer.disconnect();
    }

    [Fact]
    public void PerformanceObserver_supportedEntryTypes_ShouldReturnTypes()
    {
        var types = PerformanceObserver.supportedEntryTypes();

        Assert.NotEmpty(types);
        Assert.Contains("mark", types);
        Assert.Contains("measure", types);
    }

    [Fact]
    public void PerformanceObserverEntryList_getEntries_ShouldReturnAllEntries()
    {
        var entries = new PerformanceEntry[]
        {
            new PerformanceMark("mark1", 100.0),
            new PerformanceMark("mark2", 200.0)
        };

        var list = new PerformanceObserverEntryList(entries);

        var result = list.getEntries();

        Assert.Equal(2, result.Length);
        Assert.Equal("mark1", result[0].name);
        Assert.Equal("mark2", result[1].name);
    }

    [Fact]
    public void PerformanceObserverEntryList_getEntriesByName_ShouldFilterByName()
    {
        var entries = new PerformanceEntry[]
        {
            new PerformanceMark("test", 100.0),
            new PerformanceMark("other", 200.0),
            new PerformanceMeasure("test", 300.0, 50.0)
        };

        var list = new PerformanceObserverEntryList(entries);

        var result = list.getEntriesByName("test");

        Assert.Equal(2, result.Length);
        Assert.All(result, e => Assert.Equal("test", e.name));
    }

    [Fact]
    public void PerformanceObserverEntryList_getEntriesByName_WithType_ShouldFilterByBoth()
    {
        var entries = new PerformanceEntry[]
        {
            new PerformanceMark("test", 100.0),
            new PerformanceMeasure("test", 200.0, 50.0)
        };

        var list = new PerformanceObserverEntryList(entries);

        var result = list.getEntriesByName("test", "mark");

        Assert.Single(result);
        Assert.Equal("test", result[0].name);
        Assert.Equal("mark", result[0].entryType);
    }

    [Fact]
    public void PerformanceObserverEntryList_getEntriesByName_WithNullName_ShouldThrow()
    {
        var list = new PerformanceObserverEntryList(Array.Empty<PerformanceEntry>());

        Assert.Throws<ArgumentException>(() => list.getEntriesByName(null!));
    }

    [Fact]
    public void PerformanceObserverEntryList_getEntriesByType_ShouldFilterByType()
    {
        var entries = new PerformanceEntry[]
        {
            new PerformanceMark("mark1", 100.0),
            new PerformanceMark("mark2", 200.0),
            new PerformanceMeasure("measure1", 300.0, 50.0)
        };

        var list = new PerformanceObserverEntryList(entries);

        var marks = list.getEntriesByType("mark");
        var measures = list.getEntriesByType("measure");

        Assert.Equal(2, marks.Length);
        Assert.All(marks, e => Assert.Equal("mark", e.entryType));
        Assert.Single(measures);
        Assert.Equal("measure", measures[0].entryType);
    }

    [Fact]
    public void PerformanceObserverEntryList_getEntriesByType_WithNullType_ShouldThrow()
    {
        var list = new PerformanceObserverEntryList(Array.Empty<PerformanceEntry>());

        Assert.Throws<ArgumentException>(() => list.getEntriesByType(null!));
    }

    [Fact]
    public void PerformanceObserver_MultipleObservers_ShouldAllReceiveNotifications()
    {
        performance.clearMarks();

        var received1 = new List<PerformanceEntry>();
        var received2 = new List<PerformanceEntry>();

        var observer1 = new PerformanceObserver((list, obs) =>
        {
            received1.AddRange(list.getEntries());
        });

        var observer2 = new PerformanceObserver((list, obs) =>
        {
            received2.AddRange(list.getEntries());
        });

        observer1.observe(new PerformanceObserverOptions { entryTypes = new[] { "mark" } });
        observer2.observe(new PerformanceObserverOptions { entryTypes = new[] { "mark" } });

        performance.mark("test-mark");
        Thread.Sleep(10);

        Assert.Single(received1);
        Assert.Single(received2);
        Assert.Equal("test-mark", received1[0].name);
        Assert.Equal("test-mark", received2[0].name);

        observer1.disconnect();
        observer2.disconnect();
    }
}
