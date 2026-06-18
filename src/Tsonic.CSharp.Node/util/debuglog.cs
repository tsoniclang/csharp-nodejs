namespace Tsonic.CSharp.Node;

/// <summary>
/// A function used for conditional debug logging.
/// </summary>
public delegate void DebugLogFunction(string message, params object[] args);

public static partial class util
{
    /// <summary>
    /// Creates a function that conditionally writes debug messages to stderr
    /// based on the existence of the NODE_DEBUG environment variable.
    /// </summary>
    /// <param name="section">A string identifying the portion of the application for which the debuglog function is being created.</param>
    /// <returns>A logging function.</returns>
    public static DebugLogFunction debuglog(string section)
    {
        var nodeDebug = Environment.GetEnvironmentVariable("NODE_DEBUG") ?? "";
        var sections = nodeDebug.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(s => s.Trim().ToUpperInvariant())
                                .ToHashSet();

        var enabled = sections.Contains(section.ToUpperInvariant()) || sections.Contains("*");

        if (enabled)
        {
            var pid = Environment.ProcessId;
            return (message, args) =>
            {
                var formatted = args.Length > 0 ? string.Format(message, args) : message;
                Console.Error.WriteLine($"{section.ToUpperInvariant()} {pid}: {formatted}");
            };
        }
        else
        {
            return (message, args) => { }; // No-op
        }
    }
}
