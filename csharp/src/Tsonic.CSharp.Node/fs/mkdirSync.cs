namespace Tsonic.CSharp.Node;

public static partial class fs
{
    private static void ParseMkdirOptions(object? options, out bool recursive, out int? mode)
    {
        recursive = false;
        mode = null;

        if (options is null) return;

        if (options is bool recursiveBool)
        {
            recursive = recursiveBool;
            return;
        }

        if (options is MkdirOptions typed)
        {
            recursive = typed.recursive ?? false;
            mode = typed.mode;
            return;
        }

        if (options is IReadOnlyDictionary<string, object?> roDict)
        {
            if (roDict.TryGetValue("recursive", out var recursiveRaw))
            {
                recursive = recursiveRaw switch
                {
                    bool b => b,
                    _ => recursive
                };
            }

            if (roDict.TryGetValue("mode", out var modeRaw))
            {
                mode = modeRaw switch
                {
                    int i => i,
                    long l => (int)l,
                    short s => s,
                    byte b => b,
                    _ => mode
                };
            }
            return;
        }

        if (options is IDictionary<string, object?> dict)
        {
            if (dict.TryGetValue("recursive", out var recursiveRaw))
            {
                recursive = recursiveRaw switch
                {
                    bool b => b,
                    _ => recursive
                };
            }

            if (dict.TryGetValue("mode", out var modeRaw))
            {
                mode = modeRaw switch
                {
                    int i => i,
                    long l => (int)l,
                    short s => s,
                    byte b => b,
                    _ => mode
                };
            }
        }
    }

    /// <summary>
    /// Synchronously creates a directory.
    /// </summary>
    /// <param name="path">The directory path to create.</param>
    /// <param name="recursive">If true, creates parent directories as needed (default: false).</param>
    public static void mkdirSync(string path, bool recursive = false)
    {
        if (recursive)
        {
            Directory.CreateDirectory(path);
        }
        else
        {
            // Non-recursive: fail if parent doesn't exist
            var parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
            {
                throw new DirectoryNotFoundException($"Parent directory does not exist: {parent}");
            }
            Directory.CreateDirectory(path);
        }
    }

    /// <summary>
    /// Synchronously creates a directory using options object semantics.
    /// </summary>
    /// <param name="path">The directory path to create.</param>
    /// <param name="options">mkdir options ({ recursive?, mode? }).</param>
    public static void mkdirSync(string path, MkdirOptions? options)
    {
        mkdirSync(path, (object?)options);
    }

    /// <summary>
    /// Synchronously creates a directory using object options semantics.
    /// Accepts either a boolean recursive flag or an options object with
    /// { recursive?, mode? }.
    /// </summary>
    /// <param name="path">The directory path to create.</param>
    /// <param name="options">A recursive flag or options object.</param>
    public static void mkdirSync(string path, object? options)
    {
        ParseMkdirOptions(options, out var recursive, out var mode);
        mkdirSync(path, recursive);

        if (mode is not int modeValue) return;
        if (OperatingSystem.IsWindows()) return;

        try
        {
            File.SetUnixFileMode(path, (UnixFileMode)modeValue);
        }
        catch
        {
            // Best-effort parity with Node.js: ignore unsupported chmod failures.
        }
    }
}
