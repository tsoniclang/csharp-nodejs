using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

#pragma warning disable IDE1006

public static class fs_promises
{
    public static Task access(string path, int mode = 0) => fs.promises.access(path, mode);
    public static Task appendFile(string path, string data, string? encoding = "utf-8") => fs.promises.appendFile(path, data, encoding);
    public static Task appendFile(string path, Buffer data) => fs.promises.appendFile(path, data);
    public static Task chmod(string path, int mode) => fs.promises.chmod(path, mode);
    public static Task chown(string path, int uid, int gid) => fs.chown(path, uid, gid);
    public static Task lchown(string path, int uid, int gid) => fs.lchown(path, uid, gid);
    public static Task copyFile(string src, string dest, int mode = 0) => fs.promises.copyFile(src, dest, mode);
    public static Task cp(string src, string dest, bool recursive = false) => fs.promises.cp(src, dest, recursive);
    public static Task mkdir(string path, bool recursive = false) => fs.promises.mkdir(path, recursive);
    public static async Task<FileHandle> open(string path, string flags, int? mode = null) => new(await fs.promises.open(path, flags, mode).ConfigureAwait(false));
    public static Task<string[]> readdir(string path) => fs.promises.readdir(path);
    public static Task<object[]> readdir(string path, bool withFileTypes) => fs.promises.readdir(path, withFileTypes);
    public static Task<Dirent[]> readdirDirents(string path) => fs.promises.readdirDirents(path);
    public static Task<Dir> opendir(string path) => fs.opendir(path);
    public static Task<Buffer> readFile(string path) => fs.promises.readFile(path);
    public static Task<string> readFile(string path, string encoding) => fs.promises.readFile(path, encoding);
    public static Task<byte[]> readFileBytes(string path) => fs.promises.readFileBytes(path);
    public static Task<string> readlink(string path) => fs.promises.readlink(path);
    public static Task<string> realpath(string path) => fs.promises.realpath(path);
    public static Task rename(string oldPath, string newPath) => fs.promises.rename(oldPath, newPath);
    public static Task rm(string path, bool recursive = false) => fs.promises.rm(path, recursive);
    public static Task rmdir(string path, bool recursive = false) => fs.promises.rmdir(path, recursive);
    public static Task<Stats> stat(string path) => fs.promises.stat(path);
    public static Task<Stats> lstat(string path) => fs.lstat(path);
    public static Task<StatsFsBase> statfs(string path) => fs.statfs(path);
    public static Task symlink(string target, string path, string? type = null) => fs.promises.symlink(target, path, type);
    public static Task truncate(string path, long len = 0) => fs.promises.truncate(path, len);
    public static Task utimes(string path, DateTime atime, DateTime mtime) => fs.utimes(path, atime, mtime);
    public static Task lutimes(string path, DateTime atime, DateTime mtime) => fs.lutimes(path, atime, mtime);
    public static Task<string> mkdtemp(string prefix) => fs.mkdtemp(prefix);
    public static Task<DisposableTempDir> mkdtempDisposable(string prefix) => fs.mkdtempDisposable(prefix);
    public static Task link(string existingPath, string newPath) => fs.link(existingPath, newPath);
    public static Task<string[]> glob(string pattern) => fs.glob(pattern);
    public static Task unlink(string path) => fs.promises.unlink(path);
    public static Task writeFile(string path, string data, string? encoding = "utf-8") => fs.promises.writeFile(path, data, encoding);
    public static Task writeFile(string path, Buffer data) => fs.promises.writeFile(path, data);
    public static Task writeFileBytes(string path, Buffer data) => fs.promises.writeFileBytes(path, data);
}
