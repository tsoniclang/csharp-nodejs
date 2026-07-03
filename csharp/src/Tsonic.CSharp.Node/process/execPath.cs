namespace Tsonic.CSharp.Node;

public static partial class process
{
    /// <summary>
    /// The process.execPath property returns the absolute pathname of the executable that started the process.
    /// </summary>
    public static string execPath
    {
        get
        {
            // Try to get the process path
            var path = Environment.ProcessPath;
            if (!string.IsNullOrEmpty(path))
            {
                return path;
            }

            // Fallback to command line args
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 0)
            {
                return Path.GetFullPath(args[0]);
            }

            // Last resort fallback
            return string.Empty;
        }
    }
}
