#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsonic.CSharp.Node;

public sealed class MIMEParams : IEnumerable<KeyValuePair<string, string>>
{
    private readonly List<KeyValuePair<string, string>> _entries = [];

    public MIMEParams()
    {
    }

    internal MIMEParams(string parameters)
    {
        Parse(parameters);
    }

    public string? get(string name)
    {
        var entry = _entries.FirstOrDefault(item => string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase));
        return entry.Key == null ? null : entry.Value;
    }

    public bool has(string name)
    {
        return _entries.Any(item => string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase));
    }

    public void set(string name, string value)
    {
        ValidateToken(name);
        delete(name);
        _entries.Add(new KeyValuePair<string, string>(name.ToLowerInvariant(), value));
    }

    public void delete(string name)
    {
        _entries.RemoveAll(item => string.Equals(item.Key, name, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<KeyValuePair<string, string>> entries() => _entries;

    public IEnumerable<string> keys() => _entries.Select(item => item.Key);

    public IEnumerable<string> values() => _entries.Select(item => item.Value);

    public override string ToString()
    {
        if (_entries.Count == 0)
            return string.Empty;

        var builder = new StringBuilder();
        foreach (var (key, value) in _entries)
        {
            builder.Append("; ");
            builder.Append(key);
            builder.Append('=');
            builder.Append(QuoteIfNeeded(value));
        }

        return builder.ToString();
    }

    public IEnumerator<KeyValuePair<string, string>> GetEnumerator() => _entries.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private void Parse(string parameters)
    {
        foreach (var rawPart in parameters.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var part = rawPart.Trim();
            if (part.Length == 0)
                continue;

            var equals = part.IndexOf('=');
            if (equals <= 0)
                continue;

            var name = part[..equals].Trim();
            var value = part[(equals + 1)..].Trim();
            if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
                value = value[1..^1].Replace("\\\"", "\"", StringComparison.Ordinal);

            set(name, value);
        }
    }

    private static void ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Any(ch => char.IsControl(ch) || ch is '(' or ')' or '<' or '>' or '@' or ',' or ';' or ':' or '\\' or '"' or '/' or '[' or ']' or '?' or '='))
            throw new ArgumentException("Invalid MIME parameter name.", nameof(token));
    }

    private static string QuoteIfNeeded(string value)
    {
        if (value.Length == 0 || value.Any(ch => char.IsWhiteSpace(ch) || ch is '"' or ';' or '\\'))
            return "\"" + value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";

        return value;
    }
}

public sealed class MIMEType
{
    public MIMEType(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("MIME type is required.", nameof(input));

        var semicolon = input.IndexOf(';');
        var essenceText = semicolon >= 0 ? input[..semicolon].Trim() : input.Trim();
        var slash = essenceText.IndexOf('/');
        if (slash <= 0 || slash == essenceText.Length - 1)
            throw new FormatException("Invalid MIME type.");

        type = NormalizeToken(essenceText[..slash]);
        subtype = NormalizeToken(essenceText[(slash + 1)..]);
        @params = new MIMEParams(semicolon >= 0 ? input[semicolon..] : string.Empty);
    }

    public string essence => $"{type}/{subtype}";

    public string type { get; set; }

    public string subtype { get; set; }

    public MIMEParams @params { get; }

    public MIMEParams paramsMut => @params;

    public override string ToString() => essence + @params.ToString();

    private static string NormalizeToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new FormatException("Invalid MIME type token.");

        return token.Trim().ToLowerInvariant();
    }
}
