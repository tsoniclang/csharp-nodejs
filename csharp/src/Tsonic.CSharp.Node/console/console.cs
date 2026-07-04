using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Provides a simple debugging console similar to the JavaScript console mechanism provided by web browsers.
/// </summary>
public static class console
{
    private static readonly Dictionary<string, int> _counters = new Dictionary<string, int>();
    private static readonly Dictionary<string, Stopwatch> _timers = new Dictionary<string, Stopwatch>();
    private static readonly ConsoleConstructor _consoleCtor = new();
    private static int _groupIndentation = 0;
    private static readonly int _groupIndentationSize = 2;

    /// <summary>
    /// Console constructor-like export for compatibility.
    /// </summary>
    public static ConsoleConstructor Console => _consoleCtor;

    /// <summary>
    /// Writes a message if value is falsy or omitted. It only writes a message and does not otherwise affect execution.
    /// </summary>
    /// <param name="value">The value tested for being truthy.</param>
    /// <param name="message">Error message.</param>
    /// <param name="optionalParams">Additional parameters to include in the message.</param>
    public static void assert(bool value, string? message = null, params object[] optionalParams)
    {
        if (!value)
        {
            string fullMessage = "Assertion failed";
            if (!string.IsNullOrEmpty(message))
            {
                fullMessage += ": " + FormatMessage(message, optionalParams);
            }
            Error(fullMessage);
        }
    }

    /// <summary>
    /// When stdout is a TTY, calling console.clear() will attempt to clear the TTY. When stdout is not a TTY, this method does nothing.
    /// </summary>
    public static void clear()
    {
        try
        {
            global::System.Console.Clear();
        }
        catch
        {
            // If clearing fails (not a TTY), do nothing
        }
    }

    /// <summary>
    /// Maintains an internal counter specific to label and outputs to stdout the number of times console.count() has been called with the given label.
    /// </summary>
    /// <param name="label">The display label for the counter.</param>
    public static void count(string? label = null)
    {
        label ??= "default";

        if (!_counters.ContainsKey(label))
        {
            _counters[label] = 0;
        }

        _counters[label]++;
        WriteLine($"{label}: {_counters[label]}");
    }

    /// <summary>
    /// Resets the internal counter specific to label.
    /// </summary>
    /// <param name="label">The display label for the counter.</param>
    public static void countReset(string? label = null)
    {
        label ??= "default";
        _counters[label] = 0;
    }

    /// <summary>
    /// The console.debug() function is an alias for console.log().
    /// </summary>
    public static void debug(object? message = null, params object[] optionalParams)
    {
        log(message, optionalParams);
    }

    /// <summary>
    /// Uses util.inspect() on obj and prints the resulting string to stdout.
    /// </summary>
    public static void dir(object? obj, params object[] options)
    {
        WriteLine(util.inspect(obj));
    }

    /// <summary>
    /// This method calls console.log() passing it the arguments received. This method does not produce any XML formatting.
    /// </summary>
    public static void dirxml(params object[] data)
    {
        log(null, data);
    }

    /// <summary>
    /// Prints to stderr with newline. Multiple arguments can be passed, with the first used as the primary message.
    /// </summary>
    public static void error(object? message = null, params object[] optionalParams)
    {
        Error(FormatMessage(ConvertToString(message), optionalParams));
    }

    /// <summary>
    /// Increases indentation of subsequent lines by spaces for groupIndentation length.
    /// </summary>
    public static void group(params object[] label)
    {
        if (label.Length > 0)
        {
            WriteLine(FormatMessage(ConvertToString(label[0]), label.Length > 1 ? label[1..] : Array.Empty<object>()));
        }
        _groupIndentation++;
    }

    /// <summary>
    /// An alias for console.group().
    /// </summary>
    public static void groupCollapsed(params object[] label)
    {
        group(label);
    }

    /// <summary>
    /// Decreases indentation of subsequent lines by spaces for groupIndentation length.
    /// </summary>
    public static void groupEnd()
    {
        if (_groupIndentation > 0)
        {
            _groupIndentation--;
        }
    }

    /// <summary>
    /// The console.info() function is an alias for console.log().
    /// </summary>
    public static void info(object? message = null, params object[] optionalParams)
    {
        log(message, optionalParams);
    }

    /// <summary>
    /// Prints to stdout with newline. Multiple arguments can be passed, with the first used as the primary message.
    /// </summary>
    public static void log(object? message = null, params object[] optionalParams)
    {
        WriteLine(FormatMessage(ConvertToString(message), optionalParams));
    }

    /// <summary>
    /// Try to construct a table with the columns of the properties of tabularData and log it.
    /// </summary>
    public static void table(object? tabularData, string[]? properties = null)
    {
        // Simplified implementation - just log the object
        WriteLine(util.inspect(tabularData));
    }

    /// <summary>
    /// Starts a timer that can be used to compute the duration of an operation.
    /// </summary>
    /// <param name="label">Label for the timer.</param>
    public static void time(string? label = null)
    {
        label ??= "default";

        if (!_timers.ContainsKey(label))
        {
            var sw = new Stopwatch();
            sw.Start();
            _timers[label] = sw;
        }
    }

    /// <summary>
    /// Stops a timer that was previously started by calling console.time() and prints the result to stdout.
    /// </summary>
    /// <param name="label">Label for the timer.</param>
    public static void timeEnd(string? label = null)
    {
        label ??= "default";

        if (_timers.TryGetValue(label, out var sw))
        {
            sw.Stop();
            WriteLine($"{label}: {FormatTime(sw.Elapsed)}");
            _timers.Remove(label);
        }
    }

    /// <summary>
    /// For a timer that was previously started by calling console.time(), prints the elapsed time and other data arguments to stdout.
    /// </summary>
    /// <param name="label">Label for the timer.</param>
    /// <param name="data">Additional data to log.</param>
    public static void timeLog(string? label = null, params object[] data)
    {
        label ??= "default";

        if (_timers.TryGetValue(label, out var sw))
        {
            var message = $"{label}: {FormatTime(sw.Elapsed)}";
            if (data.Length > 0)
            {
                message += " " + string.Join(" ", Array.ConvertAll(data, obj => ConvertToString(obj)));
            }
            WriteLine(message);
        }
    }

    /// <summary>
    /// Prints to stderr the string 'Trace: ', followed by the formatted message and stack trace to the current position in the code.
    /// </summary>
    public static void trace(object? message = null, params object[] optionalParams)
    {
        var formattedMessage = FormatMessage(ConvertToString(message), optionalParams);
        var stackTrace = Environment.StackTrace;
        Error($"Trace: {formattedMessage}\n{stackTrace}");
    }

    /// <summary>
    /// The console.warn() function is an alias for console.error().
    /// </summary>
    public static void warn(object? message = null, params object[] optionalParams)
    {
        error(message, optionalParams);
    }

    // Inspector mode only (no-ops for now)

    /// <summary>
    /// This method does not display anything unless used in the inspector.
    /// </summary>
    public static void profile(string? label = null)
    {
        // No-op in non-inspector mode
    }

    /// <summary>
    /// This method does not display anything unless used in the inspector.
    /// </summary>
    public static void profileEnd(string? label = null)
    {
        // No-op in non-inspector mode
    }

    /// <summary>
    /// This method does not display anything unless used in the inspector.
    /// </summary>
    public static void timeStamp(string? label = null)
    {
        // No-op in non-inspector mode
    }

    // Helper methods

    private static void WriteLine(string message)
    {
        if (_groupIndentation > 0)
        {
            var indent = new string(' ', _groupIndentation * _groupIndentationSize);
            message = indent + message;
        }
        global::System.Console.WriteLine(message);
    }

    private static void Error(string message)
    {
        if (_groupIndentation > 0)
        {
            var indent = new string(' ', _groupIndentation * _groupIndentationSize);
            message = indent + message;
        }
        global::System.Console.Error.WriteLine(message);
    }

    private static string FormatMessage(string? message, object[] optionalParams)
    {
        if (string.IsNullOrEmpty(message) && optionalParams.Length == 0)
            return "";

        if (optionalParams.Length == 0)
            return message ?? "";

        // Simple formatting - not full printf-style
        var result = message ?? "";
        int paramIndex = 0;

        for (int i = 0; i < result.Length - 1 && paramIndex < optionalParams.Length; i++)
        {
            if (result[i] == '%')
            {
                char specifier = result[i + 1];
                string replacement = "";

                switch (specifier)
                {
                    case 's': // string
                        replacement = ConvertToString(optionalParams[paramIndex++]);
                        result = result.Remove(i, 2).Insert(i, replacement);
                        i += replacement.Length - 1;
                        break;
                    case 'd': // number
                    case 'i': // integer
                        replacement = ConvertToString(optionalParams[paramIndex++]);
                        result = result.Remove(i, 2).Insert(i, replacement);
                        i += replacement.Length - 1;
                        break;
                    case 'f': // float
                        replacement = ConvertToString(optionalParams[paramIndex++]);
                        result = result.Remove(i, 2).Insert(i, replacement);
                        i += replacement.Length - 1;
                        break;
                    case 'o': // object
                    case 'O': // object
                        replacement = util.inspect(optionalParams[paramIndex++]);
                        result = result.Remove(i, 2).Insert(i, replacement);
                        i += replacement.Length - 1;
                        break;
                    case '%': // escaped %
                        result = result.Remove(i, 2).Insert(i, "%");
                        break;
                }
            }
        }

        // Append any remaining parameters
        for (int i = paramIndex; i < optionalParams.Length; i++)
        {
            result += " " + ConvertToString(optionalParams[i]);
        }

        return result;
    }

    private static string ConvertToString(object? obj)
    {
        if (obj == null)
            return "";

        if (obj is string str)
            return str;

        return util.inspect(obj);
    }

    private static string FormatTime(TimeSpan elapsed)
    {
        if (elapsed.TotalMilliseconds < 1)
            return $"{elapsed.TotalMilliseconds:F3}ms";
        else if (elapsed.TotalSeconds < 1)
            return $"{elapsed.TotalMilliseconds:F0}ms";
        else if (elapsed.TotalMinutes < 1)
            return $"{elapsed.TotalSeconds:F3}s";
        else
            return $"{elapsed.TotalMinutes:F2}m";
    }
}

#pragma warning disable CS1591

public class ConsoleConstructor
{
    public ConsoleConstructor(object? stdout = null, object? stderr = null, bool ignoreErrors = true, object? colorMode = null, int? inspectOptions = null, bool groupIndentation = true)
    {
        _ = stdout;
        _ = stderr;
        _ = ignoreErrors;
        _ = colorMode;
        _ = inspectOptions;
        _ = groupIndentation;
    }

    public void assert(bool value, string? message = null, params object[] optionalParams) => console.assert(value, message, optionalParams);
    public void clear() => console.clear();
    public void count(string? label = null) => console.count(label);
    public void countReset(string? label = null) => console.countReset(label);
    public void debug(object? message = null, params object[] optionalParams) => console.debug(message, optionalParams);
    public void dir(object? obj, params object[] options) => console.dir(obj, options);
    public void dirxml(params object[] data) => console.dirxml(data);
    public void error(object? message = null, params object[] optionalParams) => console.error(message, optionalParams);
    public void group(params object[] label) => console.group(label);
    public void groupCollapsed(params object[] label) => console.groupCollapsed(label);
    public void groupEnd() => console.groupEnd();
    public void info(object? message = null, params object[] optionalParams) => console.info(message, optionalParams);
    public void log(object? message = null, params object[] optionalParams) => console.log(message, optionalParams);
    public void profile(string? label = null) => console.profile(label);
    public void profileEnd(string? label = null) => console.profileEnd(label);
    public void table(object? tabularData, string[]? properties = null) => console.table(tabularData, properties);
    public void time(string? label = null) => console.time(label);
    public void timeEnd(string? label = null) => console.timeEnd(label);
    public void timeLog(string? label = null, params object[] data) => console.timeLog(label, data);
    public void timeStamp(string? label = null) => console.timeStamp(label);
    public void trace(object? message = null, params object[] optionalParams) => console.trace(message, optionalParams);
    public void warn(object? message = null, params object[] optionalParams) => console.warn(message, optionalParams);
}

#pragma warning restore CS1591
