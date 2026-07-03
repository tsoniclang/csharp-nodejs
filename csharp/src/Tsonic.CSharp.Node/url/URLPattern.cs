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
    /// Creates a URL pattern from component initialization data.
    /// </summary>
    public URLPattern(URLPatternInit init, URLPatternOptions? options = null)
        : this(BuildPattern(init), options)
    {
    }

    /// <summary>
    /// Creates a URL pattern from a wildcard pattern string.
    /// </summary>
    public URLPattern(string pattern, URLPatternOptions? options)
    {
        if (pattern == null)
            throw new ArgumentNullException(nameof(pattern));

        var escaped = Regex.Escape(pattern).Replace("\\*", ".*", StringComparison.Ordinal);
        var regexOptions = RegexOptions.Compiled | RegexOptions.CultureInvariant;
        if (options?.ignoreCase == true)
            regexOptions |= RegexOptions.IgnoreCase;
        _regex = new Regex($"^{escaped}$", regexOptions);
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
    public URLPatternResult? exec(string input)
    {
        if (!test(input)) return null;

        return new URLPatternResult
        {
            inputs = [input],
            pathname = new URLPatternComponentResult { input = input }
        };
    }

    private static string BuildPattern(URLPatternInit init)
    {
        if (init == null)
            throw new ArgumentNullException(nameof(init));

        if (!string.IsNullOrEmpty(init.baseURL))
            return init.baseURL;

        return $"{init.protocol ?? "*"}://{init.hostname ?? "*"}{init.pathname ?? "*"}";
    }
}
