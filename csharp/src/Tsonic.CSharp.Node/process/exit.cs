namespace Tsonic.CSharp.Node;

public static partial class process
{
    /// <summary>
    /// The process.exit() method instructs Node.js to terminate the process synchronously
    /// with an exit status of code. If code is omitted, exit uses either the 'success' code 0
    /// or the value of process.exitCode if it has been set.
    /// </summary>
    /// <param name="code">The exit code. Defaults to process.exitCode when omitted.</param>
    public static void exit(int? code)
    {
        var exitCode = code ?? _exitCode ?? 0;
        Environment.Exit(exitCode);
    }

    /// <summary>
    /// The process.exit() method overload used by TypeScript source, where Node-style numeric values are JavaScript numbers.
    /// </summary>
    /// <param name="code">The JavaScript numeric exit code. Defaults to process.exitCode when omitted.</param>
    public static void exit(double? code = null)
    {
        exit(normalizeExitCode(code));
    }

    private static int? normalizeExitCode(double? code)
    {
        if (code == null)
        {
            return null;
        }
        if (double.IsNaN(code.Value) || double.IsInfinity(code.Value) || code.Value < int.MinValue || code.Value > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, "Exit code must be a finite 32-bit integer-compatible JavaScript number.");
        }
        return (int)code.Value;
    }
}
