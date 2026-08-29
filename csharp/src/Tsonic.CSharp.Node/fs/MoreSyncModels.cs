namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public sealed class ReadVResult
{
    public int bytesRead { get; set; }
    public byte[][] buffers { get; set; } = [];
}

public sealed class WriteVResult
{
    public int bytesWritten { get; set; }
    public byte[][] buffers { get; set; } = [];
}

public sealed class DisposableTempDir
{
    public string path { get; set; } = string.Empty;
    public bool removed { get; private set; }

    public void remove()
    {
        if (Directory.Exists(path))
            Directory.Delete(path, recursive: true);
        removed = true;
    }
}

public sealed class FsWatchEvent
{
    public string eventType { get; set; } = string.Empty;
    public string? filename { get; set; }
}

public sealed class WatchOptions
{
    public bool? persistent { get; set; }
    public bool? recursive { get; set; }
}

public sealed class ReadStreamOptions
{
    public string? flags { get; set; }
    public string? encoding { get; set; }
    public long? start { get; set; }
    public long? end { get; set; }
    public int? highWaterMark { get; set; }
}

public sealed class WriteStreamOptions
{
    public string? flags { get; set; }
    public string? encoding { get; set; }
    public long? start { get; set; }
    public int? highWaterMark { get; set; }
    public bool? flush { get; set; }
}

public sealed class Dir
{
    private readonly Queue<Dirent> _entries;

    public Dir(string path)
    {
        this.path = path;
        _entries = new Queue<Dirent>(fs.readdirDirentsSync(path));
    }

    public string path { get; }
    public bool closed { get; private set; }

    public Dirent? read()
    {
        if (closed)
            throw new ObjectDisposedException(nameof(Dir));
        return _entries.Count == 0 ? null : _entries.Dequeue();
    }

    public Dirent[] entries() => _entries.ToArray();
    public void close() => closed = true;
}
