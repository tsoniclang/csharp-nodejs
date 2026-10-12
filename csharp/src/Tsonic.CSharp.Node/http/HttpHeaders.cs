using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Tsonic.CSharp.Node.Http;

/// <summary>Case-insensitive request-header access with exact repeated values.</summary>
public sealed class IncomingHttpHeaders
{
    private readonly Dictionary<string, StringValues> _values;

    internal IncomingHttpHeaders(IEnumerable<KeyValuePair<string, StringValues>> headers)
    {
        _values = new Dictionary<string, StringValues>(
            headers.TryGetNonEnumeratedCount(out var count) ? count : 0,
            StringComparer.OrdinalIgnoreCase);
        foreach (var header in headers)
        {
            if (_values.TryGetValue(header.Key, out var prior))
                _values[header.Key] = StringValues.Concat(prior, header.Value);
            else
                _values.Add(header.Key, header.Value);
        }
    }

    internal IncomingHttpHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers)
        : this(headers.Select(header => new KeyValuePair<string, StringValues>(
            header.Key, new StringValues(header.Value.ToArray()))))
    {
    }

    /// <summary>Returns the repeated values for a header, or native absence.</summary>
    public StringValues? this[string name]
    {
        get
        {
            http.validateHeaderName(name);
            if (!_values.TryGetValue(name, out var values) || values.Count == 0) return null;
            return values;
        }
    }

    /// <summary>Returns the first value for a header, or native absence.</summary>
    public string? get(string name)
    {
        http.validateHeaderName(name);
        return _values.TryGetValue(name, out var values) && values.Count > 0
            ? values[0]
            : null;
    }

    /// <summary>Returns every value for a header in received order.</summary>
    public string[] getAll(string name)
    {
        http.validateHeaderName(name);
        if (!_values.TryGetValue(name, out var values) || values.Count == 0)
            return Array.Empty<string>();
        var result = new string[values.Count];
        for (var index = 0; index < result.Length; index++)
            result[index] = HeaderValues.read(values, index);
        return result;
    }

    /// <summary>Returns the distinct received header names.</summary>
    public string[] names() => _values.Keys.ToArray();

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
