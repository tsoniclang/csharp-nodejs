namespace Tsonic.CSharp.Node;

public static partial class process
{
    private static string[] _argv = Environment.GetCommandLineArgs();
    private static string _argv0 = Environment.GetCommandLineArgs().Length > 0
        ? Environment.GetCommandLineArgs()[0]
        : string.Empty;

    /// <summary>
    /// The process.argv property returns an array containing the command-line arguments passed when the Node.js process was launched.
    /// The first element will be process.execPath.
    /// The second element will be the path to the JavaScript file being executed.
    /// The remaining elements will be any additional command-line arguments.
    /// </summary>
    public static string[] argv
    {
        get => _argv;
        set => _argv = value ?? Array.Empty<string>();
    }

    /// <summary>
    /// The process.argv0 property stores a read-only copy of the original value of argv[0] passed when Node.js starts.
    /// </summary>
    public static string argv0
    {
        get => _argv0;
        set => _argv0 = value ?? string.Empty;
    }
}
