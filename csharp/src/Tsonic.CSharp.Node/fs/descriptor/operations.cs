namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public static partial class fs
{
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

    public static void futimesSync(int fd, DateTime atime, DateTime mtime)
    {
        var stream = EnsureFd(fd);
        File.SetLastAccessTime(stream.Name, atime);
        File.SetLastWriteTime(stream.Name, mtime);
    }
    public static Task futimes(int fd, DateTime atime, DateTime mtime, Action<Exception?>? callback = null) => CallbackTask(() => futimesSync(fd, atime, mtime), callback);

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

    private static FileStream EnsureFd(int fd) => FileDescriptorManager.Get(fd) ?? throw new ArgumentException($"Bad file descriptor: {fd}", nameof(fd));
}
