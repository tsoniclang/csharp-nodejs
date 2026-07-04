#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tsonic.CSharp.Node;

public sealed class PerformanceMarkOptions : MarkOptions
{
}

public sealed class PerformanceMeasureOptions : MeasureOptions
{
}

public sealed class PerformanceResourceTiming : PerformanceEntry
{
    public PerformanceResourceTiming(string name, double startTime, double duration)
        : base(name, "resource", startTime, duration)
    {
    }

    public double workerStart { get; set; }

    public string deliveryType { get; set; } = string.Empty;

    public Dictionary<string, object?> toEntry()
    {
        return toJSON();
    }

    public Dictionary<string, object?> toJSON()
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["name"] = name,
            ["entryType"] = entryType,
            ["startTime"] = startTime,
            ["duration"] = duration,
            ["workerStart"] = workerStart,
            ["deliveryType"] = deliveryType
        };
    }
}

public sealed class PerformanceNodeTiming : PerformanceEntry
{
    public PerformanceNodeTiming(double startTime)
        : base("node", "node", startTime, 0)
    {
        nodeStart = startTime;
        bootstrapComplete = startTime;
    }

    public double nodeStart { get; }

    public double bootstrapComplete { get; }
}

public sealed class Histogram
{
    private readonly List<long> _values = [];
    private long _lastRecord;
    private bool _enabled = true;

    public void record(long value)
    {
        if (!_enabled)
            return;

        if (value < 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Histogram values must be non-negative.");

        _values.Add(value);
        _lastRecord = Stopwatch.GetTimestamp();
    }

    public void recordDelta()
    {
        var now = Stopwatch.GetTimestamp();
        if (_lastRecord == 0)
        {
            _lastRecord = now;
            return;
        }

        record((long)((now - _lastRecord) * 1_000_000_000.0 / Stopwatch.Frequency));
    }

    public void add(Histogram other)
    {
        if (other == null)
            throw new ArgumentNullException(nameof(other));

        _values.AddRange(other._values);
    }

    public void reset()
    {
        _values.Clear();
        _lastRecord = 0;
    }

    public void enable() => _enabled = true;

    public void disable() => _enabled = false;

    public int count => _values.Count;

    public long countBigInt => count;

    public long min => _values.Count == 0 ? 0 : _values.Min();

    public long minBigInt => min;

    public long max => _values.Count == 0 ? 0 : _values.Max();

    public long maxBigInt => max;

    public double mean => _values.Count == 0 ? double.NaN : _values.Average();

    public double stddev
    {
        get
        {
            if (_values.Count == 0)
                return double.NaN;
            var average = mean;
            return Math.Sqrt(_values.Average(value => Math.Pow(value - average, 2)));
        }
    }

    public long exceeds => 0;

    public long exceedsBigInt => exceeds;

    public double percentile(double percentileValue)
    {
        if (_values.Count == 0)
            return double.NaN;

        var sorted = _values.OrderBy(value => value).ToArray();
        var index = (int)Math.Ceiling(percentileValue / 100.0 * sorted.Length) - 1;
        return sorted[Math.Clamp(index, 0, sorted.Length - 1)];
    }

    public long percentileBigInt(double percentileValue) => (long)percentile(percentileValue);

    public Dictionary<double, double> percentiles()
    {
        return new Dictionary<double, double>
        {
            [0] = percentile(0),
            [50] = percentile(50),
            [75] = percentile(75),
            [90] = percentile(90),
            [99] = percentile(99),
            [100] = percentile(100)
        };
    }

    public Dictionary<double, long> percentilesBigInt()
    {
        return percentiles().ToDictionary(item => item.Key, item => (long)item.Value);
    }
}

public sealed class EventLoopUtilization
{
    public double idle { get; set; }

    public double active { get; set; }

    public double utilization { get; set; }
}

public static partial class performance
{
    private static readonly List<PerformanceResourceTiming> _resourceEntries = [];
    private static int _resourceTimingBufferSize = 250;
    private static readonly PerformanceNodeTiming _nodeTiming = new(0);

    public static readonly Dictionary<string, int> constants = new(StringComparer.Ordinal)
    {
        ["NODE_PERFORMANCE_GC_MAJOR"] = 4,
        ["NODE_PERFORMANCE_GC_MINOR"] = 1,
        ["NODE_PERFORMANCE_GC_INCREMENTAL"] = 8,
        ["NODE_PERFORMANCE_GC_WEAKCB"] = 16
    };

    public static PerformanceNodeTiming nodeTiming => _nodeTiming;

    public static EventLoopUtilization eventLoopUtilization(EventLoopUtilization? previous = null)
    {
        var active = now();
        if (previous == null)
        {
            return new EventLoopUtilization
            {
                idle = 0,
                active = active,
                utilization = active <= 0 ? 0 : 1
            };
        }

        var deltaActive = Math.Max(0, active - previous.active);
        return new EventLoopUtilization
        {
            idle = 0,
            active = deltaActive,
            utilization = deltaActive <= 0 ? 0 : 1
        };
    }

    public static void clearResourceTimings()
    {
        lock (_lock)
        {
            _resourceEntries.Clear();
            _entries.RemoveAll(entry => entry.entryType == "resource");
        }
    }

    public static PerformanceResourceTiming addResourceTiming(string name, double startTime, double duration, string deliveryType = "")
    {
        var entry = new PerformanceResourceTiming(name, startTime, duration)
        {
            deliveryType = deliveryType
        };
        lock (_lock)
        {
            if (_resourceEntries.Count >= _resourceTimingBufferSize)
                _resourceEntries.RemoveAt(0);
            _resourceEntries.Add(entry);
            _entries.Add(entry);
        }

        PerformanceObserver.NotifyObservers(entry);
        return entry;
    }

    public static void setResourceTimingBufferSize(int maxSize)
    {
        if (maxSize < 0)
            throw new ArgumentOutOfRangeException(nameof(maxSize));

        _resourceTimingBufferSize = maxSize;
        lock (_lock)
        {
            while (_resourceEntries.Count > _resourceTimingBufferSize)
                _resourceEntries.RemoveAt(0);
        }
    }

    public static Histogram createHistogram()
    {
        return new Histogram();
    }

    public static Func<TResult> timerify<TResult>(Func<TResult> function, Histogram? histogram = null)
    {
        if (function == null)
            throw new ArgumentNullException(nameof(function));

        return () =>
        {
            var start = Stopwatch.GetTimestamp();
            try
            {
                return function();
            }
            finally
            {
                var elapsed = Stopwatch.GetTimestamp() - start;
                histogram?.record((long)(elapsed * 1_000_000_000.0 / Stopwatch.Frequency));
            }
        };
    }
}
