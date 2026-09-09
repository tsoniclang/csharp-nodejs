namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

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
