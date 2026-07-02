namespace Tsonic.CSharp.Node;

public static partial class util
{
    private static readonly HashSet<string> _deprecationWarnings = new HashSet<string>();

    /// <summary>
    /// Marks a function as deprecated. When the returned function is called,
    /// it will emit a deprecation warning to stderr (once per unique message).
    /// </summary>
    /// <param name="fn">The function to deprecate.</param>
    /// <param name="msg">The deprecation message.</param>
    /// <param name="code">Optional deprecation code.</param>
    /// <returns>A wrapper function that emits a deprecation warning.</returns>
    public static Func<TResult> deprecate<TResult>(Func<TResult> fn, string msg, string? code = null)
    {
        _ = fn;
        _ = msg;
        _ = code;
        throw UnsupportedOpenCarrierOperation("node:util.deprecate");
    }

    /// <summary>
    /// Marks an action as deprecated.
    /// </summary>
    /// <param name="action">The action to deprecate.</param>
    /// <param name="msg">The deprecation message.</param>
    /// <param name="code">Optional deprecation code.</param>
    /// <returns>A wrapper action that emits a deprecation warning.</returns>
    public static Action deprecate(Action action, string msg, string? code = null)
    {
        _ = action;
        _ = msg;
        _ = code;
        throw UnsupportedOpenCarrierOperation("node:util.deprecate");
    }
}
