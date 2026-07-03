using System;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Base class for all performance entries.
/// Represents a single performance metric entry in the Performance Timeline.
/// </summary>
public class PerformanceEntry
{
    /// <summary>
    /// The name of the performance entry.
    /// </summary>
    public string name { get; }

    /// <summary>
    /// The type of performance entry (e.g., 'mark', 'measure', 'function').
    /// </summary>
    public string entryType { get; }

    /// <summary>
    /// The high-resolution timestamp marking the start of the entry in milliseconds.
    /// </summary>
    public double startTime { get; }

    /// <summary>
    /// The duration of the entry in milliseconds.
    /// For marks, this is always 0.
    /// </summary>
    public double duration { get; }

    internal PerformanceEntry(string name, string entryType, double startTime, double duration)
    {
        this.name = name ?? throw new ArgumentNullException(nameof(name));
        this.entryType = entryType ?? throw new ArgumentNullException(nameof(entryType));
        this.startTime = startTime;
        this.duration = duration;
    }
}

/// <summary>
/// Represents a performance mark - a named timestamp in the performance timeline.
/// </summary>
public class PerformanceMark : PerformanceEntry
{
    /// <summary>
    /// Optional metadata associated with this mark.
    /// </summary>
    public object? detail { get; }

    /// <summary>
    /// Creates a new PerformanceMark.
    /// </summary>
    public PerformanceMark(string name, double startTime, object? detail = null)
        : base(name, "mark", startTime, 0)
    {
        this.detail = detail;
    }
}

/// <summary>
/// Represents a performance measure - the duration between two marks or timestamps.
/// </summary>
public class PerformanceMeasure : PerformanceEntry
{
    /// <summary>
    /// Optional metadata associated with this measure.
    /// </summary>
    public object? detail { get; }

    /// <summary>
    /// Creates a new PerformanceMeasure.
    /// </summary>
    public PerformanceMeasure(string name, double startTime, double duration, object? detail = null)
        : base(name, "measure", startTime, duration)
    {
        this.detail = detail;
    }
}
