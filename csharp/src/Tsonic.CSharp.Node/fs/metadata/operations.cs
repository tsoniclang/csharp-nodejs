namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public static partial class fs
{
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

    public static void utimesSync(string path, DateTime atime, DateTime mtime)
    {
        File.SetLastAccessTime(path, atime);
        File.SetLastWriteTime(path, mtime);
    }

    public static Task utimes(string path, DateTime atime, DateTime mtime, Action<Exception?>? callback = null) => CallbackTask(() => utimesSync(path, atime, mtime), callback);
    public static void lutimesSync(string path, DateTime atime, DateTime mtime) => utimesSync(path, atime, mtime);
    public static Task lutimes(string path, DateTime atime, DateTime mtime, Action<Exception?>? callback = null) => CallbackTask(() => lutimesSync(path, atime, mtime), callback);
}
