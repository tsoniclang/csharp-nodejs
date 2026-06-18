using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Minimal URLPattern implementation.
/// </summary>
public class URLPattern
{
    private readonly Regex _regex;

    /// <summary>
    /// Creates a URL pattern from a wildcard pattern string.
    /// </summary>
    public URLPattern(string pattern)
    {
        if (pattern == null)
            throw new ArgumentNullException(nameof(pattern));

        var escaped = Regex.Escape(pattern).Replace("\\*", ".*");
        _regex = new Regex($"^{escaped}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    }

    /// <summary>
    /// Tests whether input matches the pattern.
    /// </summary>
    public bool test(string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        return _regex.IsMatch(input);
    }

    /// <summary>
    /// Returns a minimal match object when input matches.
    /// </summary>
    public Dictionary<string, string>? exec(string input)
    {
        if (!test(input)) return null;

        return new Dictionary<string, string>
        {
            ["input"] = input
        };
    }
}
