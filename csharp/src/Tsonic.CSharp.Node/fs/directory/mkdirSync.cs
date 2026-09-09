namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously creates a directory.
    /// </summary>
    /// <param name="path">The directory path to create.</param>
    public static void mkdirSync(string path)
    {
        mkdirSync(path, new MakeDirectoryOptions());
    }

    /// <summary>
    /// Synchronously creates a directory with the exact Node options contract.
    /// </summary>
    public static void mkdirSync(string path, MakeDirectoryOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var recursive = options.recursive ?? false;
        var mode = options.mode is double configuredMode
            ? checked((int)RequireNonNegativeInteger(
                configuredMode,
                nameof(options.mode),
                0xFFF))
            : (int?)null;
        if (!recursive && Directory.Exists(path))
        {
            throw new IOException($"Path already exists: {path}");
        }
        if (!recursive)
        {
            var parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parent) && !Directory.Exists(parent))
            {
                throw new DirectoryNotFoundException($"Parent directory does not exist: {parent}");
            }
        }
        if (mode is int unixMode && !OperatingSystem.IsWindows())
        {
            Directory.CreateDirectory(path, (UnixFileMode)unixMode);
            return;
        }
        Directory.CreateDirectory(path);
    }
}
