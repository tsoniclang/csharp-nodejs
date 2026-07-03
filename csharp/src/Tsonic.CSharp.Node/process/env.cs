using System.Collections;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node;

/// <summary>
/// An object containing the user environment.
/// On Windows, environment variables are case-insensitive.
/// On Unix-like systems, they are case-sensitive.
/// </summary>
public class ProcessEnv : IDictionary<string, string?>
{
    private readonly Dictionary<string, string?> _env;
    private readonly StringComparer _comparer;

    /// <summary>
    /// Initializes a new instance of the ProcessEnv class with environment variables.
    /// </summary>
    public ProcessEnv()
    {
        // Use case-insensitive comparison on Windows, case-sensitive on Unix
        _comparer = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

        _env = new Dictionary<string, string?>(_comparer);

        // Load environment variables
        foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
        {
            if (entry.Key is string key && entry.Value is string value)
            {
                _env[key] = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets the value of an environment variable.
    /// </summary>
    /// <param name="key">The environment variable name.</param>
    /// <returns>The environment variable value, or null if it doesn't exist.</returns>
    public string? this[string key]
    {
        get => _env.TryGetValue(key, out var value) ? value : null;
        set
        {
            if (value == null)
            {
                _env.Remove(key);
                Environment.SetEnvironmentVariable(key, null);
            }
            else
            {
                _env[key] = value;
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    /// <summary>
    /// Gets a collection containing the environment variable names.
    /// </summary>
    public ICollection<string> Keys => _env.Keys;

    /// <summary>
    /// Gets a collection containing the environment variable values.
    /// </summary>
    public ICollection<string?> Values => _env.Values;

    /// <summary>
    /// Gets the number of environment variables.
    /// </summary>
    public int Count => _env.Count;

    /// <summary>
    /// Gets a value indicating whether the collection is read-only. Always returns false.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Adds an environment variable with the specified name and value.
    /// </summary>
    /// <param name="key">The environment variable name.</param>
    /// <param name="value">The environment variable value.</param>
    public void Add(string key, string? value)
    {
        _env.Add(key, value);
        if (value != null)
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }

    /// <summary>
    /// Determines whether the environment contains the specified variable name.
    /// </summary>
    /// <param name="key">The environment variable name.</param>
    /// <returns>True if the environment variable exists; otherwise, false.</returns>
    public bool ContainsKey(string key) => _env.ContainsKey(key);

    /// <summary>
    /// Removes the environment variable with the specified name.
    /// </summary>
    /// <param name="key">The environment variable name.</param>
    /// <returns>True if the variable was successfully removed; otherwise, false.</returns>
    public bool Remove(string key)
    {
        var removed = _env.Remove(key);
        if (removed)
        {
            Environment.SetEnvironmentVariable(key, null);
        }
        return removed;
    }

    /// <summary>
    /// Gets the value associated with the specified environment variable name.
    /// </summary>
    /// <param name="key">The environment variable name.</param>
    /// <param name="value">The value of the environment variable.</param>
    /// <returns>True if the environment variable exists; otherwise, false.</returns>
    public bool TryGetValue(string key, out string? value) => _env.TryGetValue(key, out value);

    /// <summary>
    /// Adds a key-value pair to the environment.
    /// </summary>
    /// <param name="item">The key-value pair to add.</param>
    public void Add(KeyValuePair<string, string?> item) => Add(item.Key, item.Value);

    /// <summary>
    /// Removes all environment variables.
    /// </summary>
    public void Clear()
    {
        foreach (var key in _env.Keys.ToList())
        {
            Environment.SetEnvironmentVariable(key, null);
        }
        _env.Clear();
    }

    /// <summary>
    /// Determines whether the environment contains a specific key-value pair.
    /// </summary>
    /// <param name="item">The key-value pair to locate.</param>
    /// <returns>True if the key-value pair exists; otherwise, false.</returns>
    public bool Contains(KeyValuePair<string, string?> item) => _env.Contains(item);

    /// <summary>
    /// Copies the environment variables to an array.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index at which copying begins.</param>
    public void CopyTo(KeyValuePair<string, string?>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, string?>>)_env).CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes a specific key-value pair from the environment.
    /// </summary>
    /// <param name="item">The key-value pair to remove.</param>
    /// <returns>True if the key-value pair was successfully removed; otherwise, false.</returns>
    public bool Remove(KeyValuePair<string, string?> item)
    {
        var removed = ((ICollection<KeyValuePair<string, string?>>)_env).Remove(item);
        if (removed)
        {
            Environment.SetEnvironmentVariable(item.Key, null);
        }
        return removed;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the environment variables.
    /// </summary>
    /// <returns>An enumerator for the environment variables.</returns>
    public IEnumerator<KeyValuePair<string, string?>> GetEnumerator() => _env.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public static partial class process
{
    private static readonly ProcessEnv _env = new ProcessEnv();

    /// <summary>
    /// The process.env property returns an object containing the user environment.
    /// </summary>
    public static ProcessEnv env => _env;
}
