namespace Tsonic.CSharp.Node;

public static partial class process
{
    /// <summary>
    /// The process.exit() method instructs Node.js to terminate the process synchronously
    /// with an exit status of code. If code is omitted, exit uses either the 'success' code 0
    /// or the value of process.exitCode if it has been set.
    /// </summary>
    /// <param name="code">The exit code. Defaults to 0 (success) or process.exitCode if set.</param>
    public static void exit(int? code = null)
    {
        var exitCode = code ?? _exitCode ?? 0;
        Environment.Exit(exitCode);
    }
}
