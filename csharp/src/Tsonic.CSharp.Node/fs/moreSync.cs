using System.Text;

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

public sealed class FsWatcher : EventEmitter
{
    private readonly FileSystemWatcher? _watcher;

    internal FsWatcher(FileSystemWatcher? watcher)
    {
        _watcher = watcher;
    }

    public bool closed { get; private set; }

    public void close()
    {
        if (closed)
            return;
        closed = true;
        _watcher?.Dispose();
        emit("close");
    }

    public void @ref() { }
    public void unref() { }
    public void poll() { }
}

public sealed class StatWatcher : EventEmitter
{
    public void @ref() { }
    public void unref() { }
}

public sealed class FsStreamOptions
{
    public string flags { get; set; } = "r";
    public string? encoding { get; set; }
    public int? fd { get; set; }
    public bool autoClose { get; set; } = true;
    public bool emitClose { get; set; } = true;
    public long? start { get; set; }
    public long? end { get; set; }
    public int highWaterMark { get; set; } = 64 * 1024;
    public bool flush { get; set; }
    public object? signal { get; set; }
}

public sealed class ReadStream : Readable
{
    public ReadStream(string path)
    {
        this.path = path;
        pending = false;
        if (File.Exists(path))
        {
            var bytes = File.ReadAllBytes(path);
            bytesRead = bytes.Length;
            push(bytes);
            push(null);
        }
    }

    public string path { get; }
    public bool pending { get; private set; }
    public long bytesRead { get; private set; }
}

public sealed class WriteStream : Writable
{
    public WriteStream(string path)
    {
        this.path = path;
        pending = false;
    }

    public string path { get; }
    public bool pending { get; private set; }
    public long bytesWritten { get; private set; }
}

public static partial class fs
{
    public static bool exists(string path) => File.Exists(path) || Directory.Exists(path);
    public static Task<bool> exists(string path, Action<bool> callback)
    {
        var value = exists(path);
        callback(value);
        return Task.FromResult(value);
    }

    public static Stats lstatSync(string path) => statSync(path);
    public static Task<Stats> lstat(string path, Action<Exception?, Stats>? callback = null)
    {
        try
        {
            var value = lstatSync(path);
            callback?.Invoke(null, value);
            return Task.FromResult(value);
        }
        catch (Exception ex)
        {
            callback?.Invoke(ex, null!);
            return Task.FromException<Stats>(ex);
        }
    }

    public static StatsFsBase statfsSync(string path)
    {
        var root = Path.GetPathRoot(Path.GetFullPath(path)) ?? Path.GetPathRoot(Environment.CurrentDirectory)!;
        var drive = new DriveInfo(root);
        var blockSize = 4096L;
        return new StatsFsBase
        {
            type = 0,
            bsize = blockSize,
            blocks = drive.TotalSize / blockSize,
            bfree = drive.TotalFreeSpace / blockSize,
            bavail = drive.AvailableFreeSpace / blockSize,
            files = 0,
            ffree = 0
        };
    }

    public static Task<StatsFsBase> statfs(string path, Action<Exception?, StatsFsBase>? callback = null)
    {
        try
        {
            var value = statfsSync(path);
            callback?.Invoke(null, value);
            return Task.FromResult(value);
        }
        catch (Exception ex)
        {
            callback?.Invoke(ex, null!);
            return Task.FromException<StatsFsBase>(ex);
        }
    }

    public static void chownSync(string path, int uid, int gid) { _ = path; _ = uid; _ = gid; }
    public static Task chown(string path, int uid, int gid, Action<Exception?>? callback = null) => CallbackTask(() => chownSync(path, uid, gid), callback);
    public static void lchownSync(string path, int uid, int gid) => chownSync(path, uid, gid);
    public static Task lchown(string path, int uid, int gid, Action<Exception?>? callback = null) => CallbackTask(() => lchownSync(path, uid, gid), callback);
    public static void fchownSync(int fd, int uid, int gid) { EnsureFd(fd); _ = uid; _ = gid; }
    public static Task fchown(int fd, int uid, int gid, Action<Exception?>? callback = null) => CallbackTask(() => fchownSync(fd, uid, gid), callback);
    public static void fchmodSync(int fd, int mode) { EnsureFd(fd); _ = mode; }
    public static Task fchmod(int fd, int mode, Action<Exception?>? callback = null) => CallbackTask(() => fchmodSync(fd, mode), callback);
    public static void fsyncSync(int fd) => EnsureFd(fd).Flush(flushToDisk: true);
    public static Task fsync(int fd, Action<Exception?>? callback = null) => CallbackTask(() => fsyncSync(fd), callback);
    public static void fdatasyncSync(int fd) => fsyncSync(fd);
    public static Task fdatasync(int fd, Action<Exception?>? callback = null) => CallbackTask(() => fdatasyncSync(fd), callback);
    public static void ftruncateSync(int fd, long len = 0) => EnsureFd(fd).SetLength(len);
    public static Task ftruncate(int fd, long len = 0, Action<Exception?>? callback = null) => CallbackTask(() => ftruncateSync(fd, len), callback);

    public static void utimesSync(string path, DateTime atime, DateTime mtime)
    {
        File.SetLastAccessTime(path, atime);
        File.SetLastWriteTime(path, mtime);
    }

    public static Task utimes(string path, DateTime atime, DateTime mtime, Action<Exception?>? callback = null) => CallbackTask(() => utimesSync(path, atime, mtime), callback);
    public static void lutimesSync(string path, DateTime atime, DateTime mtime) => utimesSync(path, atime, mtime);
    public static Task lutimes(string path, DateTime atime, DateTime mtime, Action<Exception?>? callback = null) => CallbackTask(() => lutimesSync(path, atime, mtime), callback);
    public static void futimesSync(int fd, DateTime atime, DateTime mtime)
    {
        var stream = EnsureFd(fd);
        File.SetLastAccessTime(stream.Name, atime);
        File.SetLastWriteTime(stream.Name, mtime);
    }
    public static Task futimes(int fd, DateTime atime, DateTime mtime, Action<Exception?>? callback = null) => CallbackTask(() => futimesSync(fd, atime, mtime), callback);

    public static string mkdtempSync(string prefix)
    {
        var path = prefix + Path.GetRandomFileName();
        Directory.CreateDirectory(path);
        return path;
    }

    public static Task<string> mkdtemp(string prefix, Action<Exception?, string>? callback = null)
    {
        try
        {
            var value = mkdtempSync(prefix);
            callback?.Invoke(null, value);
            return Task.FromResult(value);
        }
        catch (Exception ex)
        {
            callback?.Invoke(ex, string.Empty);
            return Task.FromException<string>(ex);
        }
    }

    public static DisposableTempDir mkdtempDisposableSync(string prefix) => new() { path = mkdtempSync(prefix) };
    public static Task<DisposableTempDir> mkdtempDisposable(string prefix) => Task.FromResult(mkdtempDisposableSync(prefix));
    public static void linkSync(string existingPath, string newPath)
    {
        File.Copy(existingPath, newPath, overwrite: false);
    }
    public static Task link(string existingPath, string newPath, Action<Exception?>? callback = null) => CallbackTask(() => linkSync(existingPath, newPath), callback);

    public static ReadVResult readvSync(int fd, byte[][] buffers, long? position = null)
    {
        var stream = EnsureFd(fd);
        if (position.HasValue)
            stream.Position = position.Value;
        var total = 0;
        foreach (var buffer in buffers)
            total += stream.Read(buffer, 0, buffer.Length);
        return new ReadVResult { bytesRead = total, buffers = buffers };
    }

    public static WriteVResult writevSync(int fd, byte[][] buffers, long? position = null)
    {
        var stream = EnsureFd(fd);
        if (position.HasValue)
            stream.Position = position.Value;
        var total = 0;
        foreach (var buffer in buffers)
        {
            stream.Write(buffer, 0, buffer.Length);
            total += buffer.Length;
        }
        return new WriteVResult { bytesWritten = total, buffers = buffers };
    }

    public static Dirent[] readdirDirentsSync(string path)
    {
        return Directory.GetFileSystemEntries(path)
            .Select(entry => new Dirent
            {
                name = Path.GetFileName(entry),
                parentPath = path,
                fileType = Directory.Exists(entry) ? "directory" : File.GetAttributes(entry).HasFlag(FileAttributes.ReparsePoint) ? "symlink" : "file"
            })
            .ToArray();
    }

    public static Dir opendirSync(string path) => new(path);
    public static Task<Dir> opendir(string path) => Task.FromResult(opendirSync(path));
    public static string[] globSync(string pattern) => Directory.GetFiles(Environment.CurrentDirectory, pattern, SearchOption.AllDirectories);
    public static Task<string[]> glob(string pattern) => Task.FromResult(globSync(pattern));
    public static FsWatcher watch(string path, Action<string, string?>? listener = null)
    {
        var directory = Directory.Exists(path) ? path : Path.GetDirectoryName(Path.GetFullPath(path)) ?? Environment.CurrentDirectory;
        var filter = Directory.Exists(path) ? "*.*" : Path.GetFileName(path);
        var watcher = new FileSystemWatcher(directory, filter);
        if (listener != null)
        {
            watcher.Changed += (_, e) => listener("change", e.Name);
            watcher.Created += (_, e) => listener("rename", e.Name);
            watcher.Deleted += (_, e) => listener("rename", e.Name);
            watcher.Renamed += (_, e) => listener("rename", e.Name);
        }
        watcher.EnableRaisingEvents = true;
        return new FsWatcher(watcher);
    }
    public static StatWatcher watchFile(string path, Action<Stats, Stats>? listener = null)
    {
        _ = path; _ = listener;
        return new StatWatcher();
    }
    public static ReadStream createReadStream(string path, FsStreamOptions? options = null) { _ = options; return new ReadStream(path); }
    public static WriteStream createWriteStream(string path, FsStreamOptions? options = null) { _ = options; return new WriteStream(path); }

    private static FileStream EnsureFd(int fd) => FileDescriptorManager.Get(fd) ?? throw new ArgumentException($"Bad file descriptor: {fd}", nameof(fd));

    private static Task CallbackTask(Action action, Action<Exception?>? callback)
    {
        try
        {
            action();
            callback?.Invoke(null);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            callback?.Invoke(ex);
            return Task.FromException(ex);
        }
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
