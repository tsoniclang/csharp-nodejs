using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Tsonic.CSharp.Node.Http;

/// <summary>Case-insensitive request-header access with exact repeated values.</summary>
public sealed class IncomingHttpHeaders
{
    private readonly IHeaderDictionary? _requestHeaders;
    private readonly Dictionary<string, string[]>? _snapshot;
    private readonly string[]? _snapshotNames;

    internal IncomingHttpHeaders(IHeaderDictionary headers)
    {
        _requestHeaders = headers;
    }

    internal IncomingHttpHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        _snapshot = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var names = new List<string>();
        foreach (var header in headers)
        {
            var key = header.Key;
            var current = header.Value.ToArray();
            if (_snapshot.TryGetValue(key, out var prior))
                _snapshot[key] = [.. prior, .. current];
            else
            {
                _snapshot.Add(key, current);
                names.Add(key);
            }
        }
        _snapshotNames = names.ToArray();
    }

    /// <summary>Returns the repeated values for a header, or native absence.</summary>
    public string[]? this[string name]
    {
        get
        {
            var values = getAll(name);
            return values.Length == 0 ? null : values;
        }
    }

    /// <summary>Returns the first value for a header, or native absence.</summary>
    public string? get(string name)
    {
        http.validateHeaderName(name);
        if (_requestHeaders is { } headers)
            return headers.TryGetValue(name, out var values) && values.Count > 0
                ? values[0]
                : null;
        return _snapshot!.TryGetValue(name, out var snapshot) && snapshot.Length > 0
            ? snapshot[0]
            : null;
    }

    /// <summary>Returns every value for a header in received order.</summary>
    public string[] getAll(string name)
    {
        http.validateHeaderName(name);
        if (_requestHeaders is { } headers)
            return headers.TryGetValue(name, out var values)
                ? values.Select(value => value ?? throw new InvalidOperationException("Native HTTP header value is null.")).ToArray()
                : Array.Empty<string>();
        return _snapshot!.TryGetValue(name, out var snapshot)
            ? [.. snapshot]
            : Array.Empty<string>();
    }

    /// <summary>Returns the distinct received header names.</summary>
    public string[] names() => _requestHeaders is { } headers
        ? headers.Keys.ToArray()
        : [.. _snapshotNames!];

}

/// <summary>Immutable response-header snapshot with exact repeated values.</summary>
public sealed class OutgoingHttpHeaders
{
    private readonly Dictionary<string, string[]> _values;
    private readonly string[] _names;

    internal OutgoingHttpHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
    {
        _values = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var names = new List<string>();
        foreach (var header in headers)
        {
            _values.Add(header.Key, header.Value.ToArray());
            names.Add(header.Key);
        }
        _names = names.ToArray();
    }

    /// <summary>Returns the first value for a header, or native absence.</summary>
    public string? get(string name)
    {
        http.validateHeaderName(name);
        return _values.TryGetValue(name, out var values) && values.Length > 0
            ? values[0]
            : null;
    }

    /// <summary>Returns every value for a header in stored order.</summary>
    public string[] getAll(string name)
    {
        http.validateHeaderName(name);
        return _values.TryGetValue(name, out var values)
            ? [.. values]
            : Array.Empty<string>();
    }

    /// <summary>Returns the distinct stored header names.</summary>
    public string[] names() => [.. _names];
}
