namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

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
    public static FsWatcher watch(string path, Action<string, string?>? listener = null) =>
        watch(path, new WatchOptions(), listener);

    public static FsWatcher watch(
        string path,
        WatchOptions options,
        Action<string, string?>? listener = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        var directory = Directory.Exists(path) ? path : Path.GetDirectoryName(Path.GetFullPath(path)) ?? Environment.CurrentDirectory;
        var filter = Directory.Exists(path) ? "*" : Path.GetFileName(path);
        var watcher = new FileSystemWatcher(directory, filter)
        {
            IncludeSubdirectories = options.recursive ?? false
        };
        var result = new FsWatcher(watcher);
        void Publish(string eventType, string? filename)
        {
            Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() =>
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
        watcher.Error += (_, e) => Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(() =>
        {
            if (!result.closed)
                result.emit("error", e.GetException());
        });
        watcher.EnableRaisingEvents = true;
        if (options.persistent == false)
            result.unref();
        return result;
    }
    public static StatWatcher watchFile(string path, Action<Stats, Stats>? listener = null)
    {
        var fullPath = Path.GetFullPath(path);
        StatWatcher? result = null;
        result = new StatWatcher(
            fullPath,
            listener,
            TimeSpan.FromMilliseconds(5007),
            () => RemoveStatWatcher(result!));
        lock (StatWatchersLock)
        {
            if (!StatWatchers.TryGetValue(fullPath, out var entries))
            {
                entries = [];
                StatWatchers.Add(fullPath, entries);
            }
            entries.Add(result);
        }
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
