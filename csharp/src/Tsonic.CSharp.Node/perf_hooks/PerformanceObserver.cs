using System;
using System.Collections.Generic;
using System.Linq;

namespace Tsonic.CSharp.Node;

/// <summary>
/// PerformanceObserver is used to observe performance measurement events and be notified
/// when new performance entries are added to the performance timeline.
/// </summary>
public class PerformanceObserver
{
    private readonly Action<PerformanceObserverEntryList, PerformanceObserver> _callback;
    private HashSet<string>? _entryTypes;
    private bool _isObserving;
    private static readonly List<PerformanceObserver> _observers = new();
    private static readonly object _observersLock = new();

    /// <summary>
    /// Creates a new PerformanceObserver instance.
    /// </summary>
    /// <param name="callback">The callback function invoked when observed performance entries are recorded.</param>
    public PerformanceObserver(Action<PerformanceObserverEntryList, PerformanceObserver> callback)
    {
        _callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    /// <summary>
    /// Starts observing performance entries of the specified types.
    /// </summary>
    /// <param name="options">Configuration options specifying which entry types to observe.</param>
    public void observe(PerformanceObserverOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        if (options.entryTypes == null || options.entryTypes.Length == 0)
            throw new ArgumentException("entryTypes must be provided and non-empty", nameof(options));

        _entryTypes = new HashSet<string>(options.entryTypes);
        _isObserving = true;

        lock (_observersLock)
        {
            if (!_observers.Contains(this))
            {
                _observers.Add(this);
            }
        }
    }

    /// <summary>
    /// Stops the PerformanceObserver from receiving performance entry notifications.
    /// </summary>
    public void disconnect()
    {
        _isObserving = false;

        lock (_observersLock)
        {
            _observers.Remove(this);
        }
    }

    /// <summary>
    /// Returns the current list of performance entries stored in the observer.
    /// This also clears the observer's buffer.
    /// </summary>
    /// <returns>A PerformanceObserverEntryList containing the buffered entries.</returns>
    public PerformanceObserverEntryList takeRecords()
    {
        // In this implementation, we don't buffer entries - observers are called immediately
        // So takeRecords returns an empty list
        return new PerformanceObserverEntryList(Array.Empty<PerformanceEntry>());
    }

    /// <summary>
    /// Internal method to notify observers of new performance entries.
    /// Called by the performance object when entries are added.
    /// </summary>
    internal static void NotifyObservers(PerformanceEntry entry)
    {
        List<PerformanceObserver> observersToNotify;

        lock (_observersLock)
        {
            observersToNotify = _observers
                .Where(o => o._isObserving && o.ShouldObserve(entry.entryType))
                .ToList();
        }

        foreach (var observer in observersToNotify)
        {
            try
            {
                var entryList = new PerformanceObserverEntryList(new[] { entry });
                observer._callback(entryList, observer);
            }
            catch
            {
                // Observers should not throw, but if they do, we continue notifying others
            }
        }
    }

    private bool ShouldObserve(string entryType)
    {
        return _entryTypes?.Contains(entryType) ?? false;
    }

    /// <summary>
    /// Returns a list of entry types that can be observed.
    /// </summary>
    /// <returns>An array of supported entry type strings.</returns>
    public static string[] supportedEntryTypes()
    {
        return new[] { "mark", "measure", "function", "gc", "resource" };
    }
}

/// <summary>
/// Options for configuring a PerformanceObserver.
/// </summary>
public class PerformanceObserverOptions
{
    /// <summary>
    /// An array of entry types to observe (e.g., 'mark', 'measure').
    /// </summary>
    public string[]? entryTypes { get; set; }

    /// <summary>
    /// If true, also notify about entries that were added before observe() was called.
    /// </summary>
    public bool buffered { get; set; }
}

/// <summary>
/// A list of performance entries provided to PerformanceObserver callbacks.
/// </summary>
public class PerformanceObserverEntryList
{
    private readonly PerformanceEntry[] _entries;

    /// <summary>
    /// Creates a new PerformanceObserverEntryList.
    /// </summary>
    public PerformanceObserverEntryList(PerformanceEntry[] entries)
    {
        _entries = entries ?? Array.Empty<PerformanceEntry>();
    }

    /// <summary>
    /// Returns all performance entries in the list.
    /// </summary>
    /// <returns>An array of all performance entries.</returns>
    public PerformanceEntry[] getEntries()
    {
        return _entries.ToArray();
    }

    /// <summary>
    /// Returns all performance entries with the given name, optionally filtered by type.
    /// </summary>
    /// <param name="name">The name to filter by.</param>
    /// <param name="type">Optional entry type to filter by.</param>
    /// <returns>An array of matching performance entries.</returns>
    public PerformanceEntry[] getEntriesByName(string name, string? type = null)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        var filtered = _entries.Where(e => e.name == name);

        if (!string.IsNullOrEmpty(type))
        {
            filtered = filtered.Where(e => e.entryType == type);
        }

        return filtered.ToArray();
    }

    /// <summary>
    /// Returns all performance entries of the given type.
    /// </summary>
    /// <param name="type">The entry type to filter by.</param>
    /// <returns>An array of matching performance entries.</returns>
    public PerformanceEntry[] getEntriesByType(string type)
    {
        if (string.IsNullOrEmpty(type))
            throw new ArgumentException("Type cannot be null or empty", nameof(type));

        return _entries.Where(e => e.entryType == type).ToArray();
    }
}
