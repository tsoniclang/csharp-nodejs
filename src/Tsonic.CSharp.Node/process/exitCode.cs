namespace Tsonic.CSharp.Node;

public static partial class process
{
    private static int? _exitCode;

    /// <summary>
    /// A number which will be the process exit code, when the process either exits gracefully,
    /// or is exited via process.exit() without specifying a code.
    ///
    /// Specifying a code to process.exit(code) will override any previous setting of process.exitCode.
    /// </summary>
    public static int? exitCode
    {
        get => _exitCode;
        set => _exitCode = value;
    }
}
