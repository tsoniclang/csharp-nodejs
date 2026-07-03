using System;
using System.IO;

namespace Tsonic.CSharp.Node;

public static partial class fs
{
    /// <summary>
    /// Synchronously opens a file and returns a file descriptor.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <param name="flags">The file system flags.</param>
    /// <param name="mode">The file mode (permissions). Default is 0o666.</param>
    /// <returns>A file descriptor (integer).</returns>
    public static int openSync(string path, string flags, int? mode = null)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("path must not be empty", nameof(path));

        var (fileMode, fileAccess) = ParseFlags(flags);

        try
        {
            var stream = new FileStream(
                path,
                fileMode,
                fileAccess,
                FileShare.ReadWrite,
                4096,
                FileOptions.None
            );

            return FileDescriptorManager.Register(stream);
        }
        catch (Exception ex)
        {
            throw new IOException($"Failed to open '{path}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Parses Node.js file flags into .NET FileMode and FileAccess.
    /// </summary>
    private static (FileMode mode, FileAccess access) ParseFlags(string flags)
    {
        return flags switch
        {
            "r" => (FileMode.Open, FileAccess.Read),
            "r+" => (FileMode.Open, FileAccess.ReadWrite),
            "rs" or "sr" => (FileMode.Open, FileAccess.Read), // Sync mode ignored
            "rs+" or "sr+" => (FileMode.Open, FileAccess.ReadWrite),

            "w" => (FileMode.Create, FileAccess.Write),
            "wx" or "xw" => (FileMode.CreateNew, FileAccess.Write),
            "w+" => (FileMode.Create, FileAccess.ReadWrite),
            "wx+" or "xw+" => (FileMode.CreateNew, FileAccess.ReadWrite),

            "a" => (FileMode.Append, FileAccess.Write),
            "ax" or "xa" => (FileMode.Append, FileAccess.Write), // CreateNew for append
            "a+" => (FileMode.Append, FileAccess.ReadWrite),
            "ax+" or "xa+" => (FileMode.Append, FileAccess.ReadWrite),

            _ => throw new ArgumentException($"Unknown file open flag: {flags}")
        };
    }
}
