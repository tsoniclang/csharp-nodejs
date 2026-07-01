using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Tsonic.CSharp.Node;

/// <summary>
/// The URLSearchParams API provides read and write access to the query of a URL.
/// </summary>
public class URLSearchParams
{
    private readonly List<KeyValuePair<string, string>> _params;
    private readonly Action<URLSearchParams>? _onChanged;

    /// <summary>
    /// Gets the total number of parameter entries.
    /// </summary>
    public int size => _params.Count;

    /// <summary>
    /// Creates a new URLSearchParams object.
    /// </summary>
    public URLSearchParams(string? init = null)
        : this(init, null)
    {
    }

    internal URLSearchParams(string? init, Action<URLSearchParams>? onChanged)
    {
        _params = new List<KeyValuePair<string, string>>();
        _onChanged = onChanged;

        if (!string.IsNullOrEmpty(init))
        {
            ParseQueryString(init);
        }
    }

    /// <summary>
    /// Appends a specified key/value pair as a new search parameter.
    /// </summary>
    public void append(string name, string value)
    {
        _params.Add(new KeyValuePair<string, string>(name, value));
        NotifyChanged();
    }

    /// <summary>
    /// Sets the value associated with a given search parameter. Replaces all existing values.
    /// </summary>
    public void set(string name, string value)
    {
        _params.RemoveAll(p => p.Key == name);
        _params.Add(new KeyValuePair<string, string>(name, value));
        NotifyChanged();
    }

    /// <summary>
    /// Returns the first value associated with the given search parameter.
    /// </summary>
    public string? get(string name)
    {
        var entry = _params.FirstOrDefault(p => p.Key == name);
        return entry.Key != null ? entry.Value : null;
    }

    /// <summary>
    /// Returns all the values associated with a given search parameter.
    /// </summary>
    public string[] getAll(string name)
    {
        return _params.Where(p => p.Key == name).Select(p => p.Value).ToArray();
    }

    /// <summary>
    /// Returns a boolean indicating if such a search parameter exists.
    /// </summary>
    public bool has(string name, string? value = null)
    {
        if (value == null)
        {
            return _params.Any(p => p.Key == name);
        }
        return _params.Any(p => p.Key == name && p.Value == value);
    }

    /// <summary>
    /// Deletes the given search parameter and all its associated values.
    /// </summary>
    public void delete(string name, string? value = null)
    {
        if (value == null)
        {
            _params.RemoveAll(p => p.Key == name);
        }
        else
        {
            _params.RemoveAll(p => p.Key == name && p.Value == value);
        }
        NotifyChanged();
    }

    /// <summary>
    /// Sorts all key/value pairs by their keys.
    /// </summary>
    public void sort()
    {
        _params.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));
        NotifyChanged();
    }

    /// <summary>
    /// Returns an iterator allowing iteration through all keys contained in this object.
    /// </summary>
    public IEnumerable<string> keys()
    {
        return _params.Select(p => p.Key);
    }

    /// <summary>
    /// Returns an iterator allowing iteration through all values contained in this object.
    /// </summary>
    public IEnumerable<string> values()
    {
        return _params.Select(p => p.Value);
    }

    /// <summary>
    /// Returns an iterator allowing iteration through all key/value pairs.
    /// </summary>
    public IEnumerable<KeyValuePair<string, string>> entries()
    {
        return _params;
    }

    /// <summary>
    /// Executes a provided function once for each key/value pair.
    /// </summary>
    public void forEach(Action<string, string> callback)
    {
        foreach (var param in _params)
        {
            callback(param.Value, param.Key);
        }
    }

    /// <summary>
    /// Returns a query string suitable for use in a URL.
    /// </summary>
    public override string ToString()
    {
        if (_params.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        for (int i = 0; i < _params.Count; i++)
        {
            if (i > 0)
                sb.Append('&');

            sb.Append(Uri.EscapeDataString(_params[i].Key));
            sb.Append('=');
            sb.Append(Uri.EscapeDataString(_params[i].Value));
        }

        return sb.ToString();
    }

    private void ParseQueryString(string query)
    {
        if (query.StartsWith("?"))
            query = query.Substring(1);

        if (string.IsNullOrEmpty(query))
            return;

        var pairs = query.Split('&');
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            var key = Uri.UnescapeDataString(parts[0]);
            var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : "";
            _params.Add(new KeyValuePair<string, string>(key, value));
        }
    }

    internal void ReplaceFromQueryString(string? query)
    {
        _params.Clear();
        if (!string.IsNullOrEmpty(query))
        {
            ParseQueryString(query);
        }
    }

    private void NotifyChanged()
    {
        _onChanged?.Invoke(this);
    }
}
