using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Tsonic.CSharp.Node;

/// <summary>
/// The performance object provides access to performance-related information.
/// It is a singleton instance that provides high-resolution timing and performance measurement.
/// </summary>
public static partial class performance
{
    private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private static readonly List<PerformanceEntry> _entries = new();
    private static readonly object _lock = new();

    /// <summary>
    /// Returns the current high-resolution timestamp in milliseconds.
    /// The timestamp is relative to an arbitrary point in time and is monotonically increasing.
    /// </summary>
    /// <returns>The current timestamp in milliseconds with sub-millisecond precision.</returns>
    public static double now()
    {
        return _stopwatch.Elapsed.TotalMilliseconds;
    }

    /// <summary>
    /// Creates a new PerformanceMark entry with the given name.
    /// The mark represents a single point in time.
    /// </summary>
    /// <param name="name">The name of the mark.</param>
    /// <param name="options">Optional configuration including detail metadata and start time.</param>
    /// <returns>The created PerformanceMark.</returns>
    public static PerformanceMark mark(string name, MarkOptions? options = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        var startTime = options?.startTime ?? now();
        var mark = new PerformanceMark(name, startTime, options?.detail);

        lock (_lock)
        {
            _entries.Add(mark);
        }

        // Notify observers
        PerformanceObserver.NotifyObservers(mark);

        return mark;
    }

    /// <summary>
    /// Creates a new PerformanceMeasure entry representing the duration between two marks or timestamps.
    /// </summary>
    /// <param name="name">The name of the measure.</param>
    /// <param name="startOrOptions">Either the name of the start mark, or options object.</param>
    /// <param name="endMark">The name of the end mark (optional if using options).</param>
    /// <returns>The created PerformanceMeasure.</returns>
    public static PerformanceMeasure measure(string name, object? startOrOptions = null, string? endMark = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        double startTime;
        double endTime;
        object? detail = null;

        // Check if first argument is options object
        if (startOrOptions is MeasureOptions options)
        {
            // Options-based measure
            startTime = options.start ?? GetMarkTime(options.startMark) ?? 0;
            endTime = options.end ?? GetMarkTime(options.endMark) ?? now();
            detail = options.detail;
        }
        else
        {
            // Mark-based measure
            var startMarkName = startOrOptions as string;
            startTime = GetMarkTime(startMarkName) ?? 0;
            endTime = GetMarkTime(endMark) ?? now();
        }

        var duration = endTime - startTime;
        var measure = new PerformanceMeasure(name, startTime, duration, detail);

        lock (_lock)
        {
            _entries.Add(measure);
        }

        // Notify observers
        PerformanceObserver.NotifyObservers(measure);

        return measure;
    }

    /// <summary>
    /// Returns all performance entries in chronological order.
    /// </summary>
    /// <returns>An array of all performance entries.</returns>
    public static PerformanceEntry[] getEntries()
    {
        lock (_lock)
        {
            return _entries.ToArray();
        }
    }

    /// <summary>
    /// Returns all performance entries with the given name, optionally filtered by type.
    /// </summary>
    /// <param name="name">The name to filter by.</param>
    /// <param name="type">Optional entry type to filter by ('mark' or 'measure').</param>
    /// <returns>An array of matching performance entries.</returns>
    public static PerformanceEntry[] getEntriesByName(string name, string? type = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        lock (_lock)
        {
            var filtered = _entries.Where(e => e.name == name);

            if (!string.IsNullOrEmpty(type))
            {
                filtered = filtered.Where(e => e.entryType == type);
            }

            return filtered.ToArray();
        }
    }

    /// <summary>
    /// Returns all performance entries of the given type.
    /// </summary>
    /// <param name="type">The entry type to filter by ('mark' or 'measure').</param>
    /// <returns>An array of matching performance entries.</returns>
    public static PerformanceEntry[] getEntriesByType(string type)
    {
        if (string.IsNullOrEmpty(type))
            throw new ArgumentException("Type cannot be null or empty", nameof(type));

        lock (_lock)
        {
            return _entries.Where(e => e.entryType == type).ToArray();
        }
    }

    /// <summary>
    /// Removes all marks from the performance timeline, or a specific mark if name is provided.
    /// </summary>
    /// <param name="name">Optional name of the mark to remove. If not provided, all marks are removed.</param>
    public static void clearMarks(string? name = null)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(name))
            {
                _entries.RemoveAll(e => e.entryType == "mark");
            }
            else
            {
                _entries.RemoveAll(e => e.entryType == "mark" && e.name == name);
            }
        }
    }

    /// <summary>
    /// Removes all measures from the performance timeline, or a specific measure if name is provided.
    /// </summary>
    /// <param name="name">Optional name of the measure to remove. If not provided, all measures are removed.</param>
    public static void clearMeasures(string? name = null)
    {
        lock (_lock)
        {
            if (string.IsNullOrEmpty(name))
            {
                _entries.RemoveAll(e => e.entryType == "measure");
            }
            else
            {
                _entries.RemoveAll(e => e.entryType == "measure" && e.name == name);
            }
        }
    }

    /// <summary>
    /// Helper method to get the timestamp of a mark by name.
    /// </summary>
    private static double? GetMarkTime(string? markName)
    {
        if (string.IsNullOrEmpty(markName))
            return null;

        lock (_lock)
        {
            var mark = _entries.LastOrDefault(e => e.entryType == "mark" && e.name == markName);
            return mark?.startTime;
        }
    }
}

/// <summary>
/// Options for creating a performance mark.
/// </summary>
public class MarkOptions
{
    /// <summary>
    /// Optional metadata to attach to the mark.
    /// </summary>
    public object? detail { get; set; }

    /// <summary>
    /// Optional start time for the mark. If not provided, uses performance.now().
    /// </summary>
    public double? startTime { get; set; }
}

/// <summary>
/// Options for creating a performance measure.
/// </summary>
public class MeasureOptions
{
    /// <summary>
    /// Optional metadata to attach to the measure.
    /// </summary>
    public object? detail { get; set; }

    /// <summary>
    /// Name of the start mark.
    /// </summary>
    public string? startMark { get; set; }

    /// <summary>
    /// Name of the end mark.
    /// </summary>
    public string? endMark { get; set; }

    /// <summary>
    /// Explicit start time (alternative to startMark).
    /// </summary>
    public double? start { get; set; }

    /// <summary>
    /// Explicit end time (alternative to endMark).
    /// </summary>
    public double? end { get; set; }
}
