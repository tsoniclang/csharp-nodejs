using System.Text;
using System.Threading;
using Tsonic.CSharp.Js;

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
    private int _closed;
    private int _referenced = 1;

    internal FsWatcher(FileSystemWatcher? watcher)
    {
        _watcher = watcher;
        ProcessKeepAlive.Acquire();
    }

    public bool closed => Volatile.Read(ref _closed) != 0;

    public void close()
    {
        if (Interlocked.Exchange(ref _closed, 1) != 0)
            return;
        _watcher?.Dispose();
        if (Interlocked.Exchange(ref _referenced, 0) != 0)
            ProcessKeepAlive.Release();
        emit("close");
    }

    internal void publish(string eventType, string? filename)
    {
        if (!closed)
            emit("change", eventType, filename);
    }

    public FsWatcher @ref()
    {
        if (!closed && Interlocked.Exchange(ref _referenced, 1) == 0)
            ProcessKeepAlive.Acquire();
        return this;
    }
    public FsWatcher unref()
    {
        if (Interlocked.Exchange(ref _referenced, 0) != 0)
            ProcessKeepAlive.Release();
        return this;
    }
    public void poll() { }
}

public sealed class StatWatcher : EventEmitter
{
    private readonly FileSystemWatcher _watcher;
    private readonly Action _onClose;
    private int _closed;
    private int _referenced = 1;

    internal StatWatcher(
        string path,
        Action<Stats, Stats>? listener,
        FileSystemWatcher watcher,
        Action onClose)
    {
        this.path = path;
        this.listener = listener;
        _watcher = watcher;
        _onClose = onClose;
        ProcessKeepAlive.Acquire();
    }

    internal string path { get; }
    internal Action<Stats, Stats>? listener { get; }
    public bool closed => Volatile.Read(ref _closed) != 0;

    public void close()
    {
        if (Interlocked.Exchange(ref _closed, 1) != 0)
            return;
        _watcher.Dispose();
        _onClose();
        if (Interlocked.Exchange(ref _referenced, 0) != 0)
            ProcessKeepAlive.Release();
        emit("close");
    }

    internal void publish(Stats current, Stats previous)
    {
        if (!closed)
        {
            listener?.Invoke(current, previous);
            emit("change", current, previous);
        }
    }

    public StatWatcher @ref()
    {
        if (!closed && Interlocked.Exchange(ref _referenced, 1) == 0)
            ProcessKeepAlive.Acquire();
        return this;
    }
    public StatWatcher unref()
    {
        if (Interlocked.Exchange(ref _referenced, 0) != 0)
            ProcessKeepAlive.Release();
        return this;
    }
}

public sealed class ReadStreamOptions
{
    public string flags { get; set; } = "r";
    public string? encoding { get; set; }
    public long? start { get; set; }
    public long? end { get; set; }
    public int highWaterMark { get; set; } = 64 * 1024;
}

public sealed class WriteStreamOptions
{
    public string flags { get; set; } = "w";
    public string? encoding { get; set; }
    public long? start { get; set; }
    public int highWaterMark { get; set; } = 64 * 1024;
    public bool flush { get; set; }
}

public sealed class ReadStream : Readable
{
    private readonly FileStream _stream;
    private readonly int _highWaterMark;
    private long? _remaining;

    public ReadStream(string path, ReadStreamOptions? options = null)
        : base(ValidateHighWaterMark(options?.highWaterMark ?? 64 * 1024))
    {
        this.path = path;
        pending = false;
        var open = FsStreamOpenOptions.ForRead(options?.flags ?? "r");
        _highWaterMark = options?.highWaterMark ?? 64 * 1024;
        _stream = new FileStream(
            path,
            open.Mode,
            open.Access,
            FileShare.ReadWrite | FileShare.Delete,
            _highWaterMark,
            open.Options);
        if (options?.start is long start)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(options), "ReadStream start must be non-negative.");
            _stream.Seek(start, SeekOrigin.Begin);
        }
        if (options?.end is long end)
        {
            var start = options.start ?? 0;
            if (end < start)
                throw new ArgumentOutOfRangeException(nameof(options), "ReadStream end must not precede start.");
            _remaining = checked(end - start + 1);
        }
        if (options?.encoding is string encoding)
            setEncoding(encoding);
    }

    private static int ValidateHighWaterMark(int value) =>
        value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));

    public string path { get; }
    public bool pending { get; private set; }
    public long bytesRead { get; private set; }

    protected override void _read(int size)
    {
        if (_remaining == 0)
        {
            _stream.Dispose();
            push(null);
            return;
        }
        var requested = Math.Max(1, Math.Min(size, _highWaterMark));
        if (_remaining is long remaining)
            requested = checked((int)Math.Min(requested, remaining));
        var bytes = new byte[requested];
        var count = _stream.Read(bytes, 0, bytes.Length);
        if (count == 0)
        {
            _stream.Dispose();
            push(null);
            return;
        }
        if (count != bytes.Length)
            Array.Resize(ref bytes, count);
        bytesRead += count;
        if (_remaining is not null)
            _remaining -= count;
        push(Buffer.from(bytes));
    }

    public WriteStream pipeTo(WriteStream destination)
    {
        _ = pipe(destination);
        return destination;
    }

    public override void destroy(Exception? error = null)
    {
        _stream.Dispose();
        base.destroy(error);
    }
}

public sealed class WriteStream : Writable
{
    private readonly FileStream _stream;
    private readonly string _defaultEncoding;
    private readonly bool _flushToDisk;

    public WriteStream(string path, WriteStreamOptions? options = null)
        : base(ValidateHighWaterMark(options?.highWaterMark ?? 64 * 1024))
    {
        this.path = path;
        pending = false;
        _defaultEncoding = options?.encoding ?? "utf-8";
        _ = Encoding.GetEncoding(_defaultEncoding);
        _flushToDisk = options?.flush == true;
        var open = FsStreamOpenOptions.ForWrite(options?.flags ?? "w");
        _stream = new FileStream(
            path,
            open.Mode,
            open.Access,
            FileShare.ReadWrite | FileShare.Delete,
            options?.highWaterMark ?? 64 * 1024,
            open.Options);
        if (open.Append)
            _stream.Seek(0, SeekOrigin.End);
        else if (options?.start is long start)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException(nameof(options), "WriteStream start must be non-negative.");
            _stream.Seek(start, SeekOrigin.Begin);
        }
    }

    private static int ValidateHighWaterMark(int value) =>
        value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));

    public string path { get; }
    public bool pending { get; private set; }
    public long bytesWritten { get; private set; }

    protected override void _write(object? chunk, string? encoding, Action callback)
    {
        var bytes = chunk switch
        {
            Buffer buffer => buffer.InternalData,
            byte[] value => value,
            string value => Encoding.GetEncoding(encoding ?? _defaultEncoding).GetBytes(value),
            _ => throw new ArgumentException("WriteStream accepts Buffer, byte[], or string chunks.", nameof(chunk))
        };
        _stream.Write(bytes, 0, bytes.Length);
        bytesWritten += bytes.Length;
        callback();
    }

    protected override void _final(Action callback)
    {
        _stream.Flush(_flushToDisk);
        _stream.Dispose();
        callback();
    }

    public override void destroy(Exception? error = null)
    {
        _stream.Dispose();
        base.destroy(error);
    }
}

public static partial class fs
{
    private static readonly object StatWatchersLock = new();
    private static readonly Dictionary<string, HashSet<StatWatcher>> StatWatchers = new(StringComparer.Ordinal);

    public static bool exists(string path) => File.Exists(path) || Directory.Exists(path);
    public static Task<bool> exists(string path, Action<bool> callback)
    {
        var value = exists(path);
        callback(value);
        return Task.FromResult(value);
    }

    public static Stats lstatSync(string path)
    {
        var attributes = File.GetAttributes(path);
        var symbolicLink = (attributes & FileAttributes.ReparsePoint) != 0;
        FileSystemInfo info = (attributes & FileAttributes.Directory) != 0
            ? new DirectoryInfo(path)
            : new FileInfo(path);
        info.Refresh();

        var size = 0L;
        if (symbolicLink)
        {
            var linkTarget = info.LinkTarget;
            if (linkTarget != null)
                size = System.Text.Encoding.UTF8.GetByteCount(linkTarget);
        }
        else if (info is FileInfo fileInfo)
        {
            size = fileInfo.Length;
        }

        return new Stats
        {
            size = size,
            mode = 0,
            atime = StatTime.ToJsDate(info.LastAccessTime),
            atimeMs = StatTime.ToUnixMilliseconds(info.LastAccessTime),
            mtime = StatTime.ToJsDate(info.LastWriteTime),
            mtimeMs = StatTime.ToUnixMilliseconds(info.LastWriteTime),
            ctime = StatTime.ToJsDate(info.CreationTime),
            ctimeMs = StatTime.ToUnixMilliseconds(info.CreationTime),
            birthtime = StatTime.ToJsDate(info.CreationTime),
            birthtimeMs = StatTime.ToUnixMilliseconds(info.CreationTime),
            isFile = !symbolicLink && info is FileInfo,
            isDirectory = !symbolicLink && info is DirectoryInfo,
            isSymbolicLink = symbolicLink
        };
    }
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
        var filter = Directory.Exists(path) ? "*" : Path.GetFileName(path);
        var watcher = new FileSystemWatcher(directory, filter);
        var result = new FsWatcher(watcher);
        void Publish(string eventType, string? filename)
        {
            Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() =>
            {
                if (result.closed)
                    return;
                listener?.Invoke(eventType, filename);
                result.publish(eventType, filename);
            });
        }
        watcher.Changed += (_, e) => Publish("change", e.Name);
        watcher.Created += (_, e) => Publish("rename", e.Name);
        watcher.Deleted += (_, e) => Publish("rename", e.Name);
        watcher.Renamed += (_, e) => Publish("rename", e.Name);
        watcher.Error += (_, e) => Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() =>
        {
            if (!result.closed)
                result.emit("error", e.GetException());
        });
        watcher.EnableRaisingEvents = true;
        return result;
    }
    public static StatWatcher watchFile(string path, Action<Stats, Stats>? listener = null)
    {
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath) ?? Environment.CurrentDirectory;
        var fileName = Path.GetFileName(fullPath);
        var watcher = new FileSystemWatcher(directory, fileName);
        var previous = File.Exists(fullPath) || Directory.Exists(fullPath)
            ? statSync(fullPath)
            : new Stats();
        StatWatcher? result = null;
        result = new StatWatcher(fullPath, listener, watcher, () => RemoveStatWatcher(result!));

        void Publish()
        {
            Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() =>
            {
                if (result.closed)
                    return;
                var current = File.Exists(fullPath) || Directory.Exists(fullPath)
                    ? statSync(fullPath)
                    : new Stats();
                var prior = previous;
                previous = current;
                result.publish(current, prior);
            });
        }

        watcher.Changed += (_, _) => Publish();
        watcher.Created += (_, _) => Publish();
        watcher.Deleted += (_, _) => Publish();
        watcher.Renamed += (_, _) => Publish();
        watcher.Error += (_, e) => Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() =>
        {
            if (!result.closed)
                result.emit("error", e.GetException());
        });
        lock (StatWatchersLock)
        {
            if (!StatWatchers.TryGetValue(fullPath, out var entries))
            {
                entries = [];
                StatWatchers.Add(fullPath, entries);
            }
            entries.Add(result);
        }
        watcher.EnableRaisingEvents = true;
        return result;
    }
    public static void unwatchFile(string path, Action<Stats, Stats>? listener = null)
    {
        var fullPath = Path.GetFullPath(path);
        StatWatcher[] matches;
        lock (StatWatchersLock)
        {
            matches = StatWatchers.TryGetValue(fullPath, out var entries)
                ? entries.Where(entry => listener is null || Equals(entry.listener, listener)).ToArray()
                : [];
        }
        foreach (var watcher in matches)
            watcher.close();
    }
    public static ReadStream createReadStream(string path, ReadStreamOptions? options = null) => new(path, options);
    public static WriteStream createWriteStream(string path, WriteStreamOptions? options = null) => new(path, options);

    private static void RemoveStatWatcher(StatWatcher watcher)
    {
        lock (StatWatchersLock)
        {
            if (!StatWatchers.TryGetValue(watcher.path, out var entries))
                return;
            entries.Remove(watcher);
            if (entries.Count == 0)
                StatWatchers.Remove(watcher.path);
        }
    }

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
