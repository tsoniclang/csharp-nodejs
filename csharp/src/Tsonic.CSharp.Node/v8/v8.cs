namespace Tsonic.CSharp.Node;

/// <summary>Explicit boundary for V8 controls unavailable in native programs.</summary>
public static class v8
{
    /// <summary>Rejects a V8 flag request without changing native process state.</summary>
    /// <param name="flags">The requested V8 flags, including an empty string.</param>
    /// <exception cref="System.PlatformNotSupportedException">The native runtime does not host V8.</exception>
    public static void setFlagsFromString(string flags)
    {
        throw new System.PlatformNotSupportedException(
            "node:v8.setFlagsFromString requires a V8 engine; native programs do not host V8");
    }
}
