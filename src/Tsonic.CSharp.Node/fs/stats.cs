namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

/// <summary>
/// Provides information about a file or directory.
/// </summary>
public class Stats
{
    public long dev { get; set; }
    public long ino { get; set; }
    public long nlink { get; set; } = 1;
    public int uid { get; set; }
    public int gid { get; set; }
    public long rdev { get; set; }
    public long blksize { get; set; } = 4096;
    public long blocks { get; set; }
    /// <summary>The size of the file in bytes (0 for directories).</summary>
    public long size { get; set; }

    /// <summary>The file mode (permissions). Not fully supported on Windows.</summary>
    public int mode { get; set; }

    /// <summary>The last access time.</summary>
    public Tsonic.CSharp.Js.Date atime { get; set; } = new(0);

    /// <summary>The last access time in Unix epoch milliseconds.</summary>
    public double atimeMs { get; set; }
    public long atimeNs { get; set; }

    /// <summary>The last modified time.</summary>
    public Tsonic.CSharp.Js.Date mtime { get; set; } = new(0);

    /// <summary>The last modified time in Unix epoch milliseconds.</summary>
    public double mtimeMs { get; set; }
    public long mtimeNs { get; set; }

    /// <summary>The last status change time.</summary>
    public Tsonic.CSharp.Js.Date ctime { get; set; } = new(0);

    /// <summary>The last status change time in Unix epoch milliseconds.</summary>
    public double ctimeMs { get; set; }
    public long ctimeNs { get; set; }

    /// <summary>The creation time (birthtime).</summary>
    public Tsonic.CSharp.Js.Date birthtime { get; set; } = new(0);

    /// <summary>The creation time in Unix epoch milliseconds.</summary>
    public double birthtimeMs { get; set; }
    public long birthtimeNs { get; set; }

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

public sealed class BigIntStats : Stats
{
}

public class StatsFsBase
{
    public long type { get; set; }
    public long bsize { get; set; }
    public long blocks { get; set; }
    public long bfree { get; set; }
    public long bavail { get; set; }
    public long files { get; set; }
    public long ffree { get; set; }
}

public sealed class BigIntStatsFs : StatsFsBase
{
}

public sealed class Dirent
{
    public string name { get; set; } = string.Empty;
    public string parentPath { get; set; } = string.Empty;
    public string fileType { get; set; } = "file";
    public bool isFile() => fileType == "file";
    public bool isDirectory() => fileType == "directory";
    public bool isSymbolicLink() => fileType == "symlink";
}
