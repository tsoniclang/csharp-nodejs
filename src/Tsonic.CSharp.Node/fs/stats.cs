namespace Tsonic.CSharp.Node;

/// <summary>
/// Provides information about a file or directory.
/// </summary>
public class Stats
{
    /// <summary>The size of the file in bytes (0 for directories).</summary>
    public long size { get; set; }

    /// <summary>The file mode (permissions). Not fully supported on Windows.</summary>
    public int mode { get; set; }

    /// <summary>The last access time.</summary>
    public Tsonic.CSharp.Js.Date atime { get; set; } = new(0);

    /// <summary>The last access time in Unix epoch milliseconds.</summary>
    public double atimeMs { get; set; }

    /// <summary>The last modified time.</summary>
    public Tsonic.CSharp.Js.Date mtime { get; set; } = new(0);

    /// <summary>The last modified time in Unix epoch milliseconds.</summary>
    public double mtimeMs { get; set; }

    /// <summary>The last status change time.</summary>
    public Tsonic.CSharp.Js.Date ctime { get; set; } = new(0);

    /// <summary>The last status change time in Unix epoch milliseconds.</summary>
    public double ctimeMs { get; set; }

    /// <summary>The creation time (birthtime).</summary>
    public Tsonic.CSharp.Js.Date birthtime { get; set; } = new(0);

    /// <summary>The creation time in Unix epoch milliseconds.</summary>
    public double birthtimeMs { get; set; }

    /// <summary>True if this is a file.</summary>
    public bool isFile { get; set; }

    /// <summary>True if this is a directory.</summary>
    public bool isDirectory { get; set; }

    /// <summary>Returns true if this is a file.</summary>
    public bool IsFile() => isFile;

    /// <summary>Returns true if this is a directory.</summary>
    public bool IsDirectory() => isDirectory;

    /// <summary>Returns true if this is a symbolic link (not supported).</summary>
    public bool IsSymbolicLink() => false;

    /// <summary>Returns true if this is a block device (not supported on Windows).</summary>
    public bool IsBlockDevice() => false;

    /// <summary>Returns true if this is a character device (not supported on Windows).</summary>
    public bool IsCharacterDevice() => false;

    /// <summary>Returns true if this is a FIFO (not supported on Windows).</summary>
    public bool IsFIFO() => false;

    /// <summary>Returns true if this is a socket (not supported on Windows).</summary>
    public bool IsSocket() => false;
}
