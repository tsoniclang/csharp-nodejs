using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public static partial class fs
{
    private static readonly FsPromises _promises = new();

    /// <summary>
    /// Promise-based fs APIs.
    /// </summary>
    public static FsPromises promises => _promises;
}

/// <summary>
/// Promise-based wrappers over fs methods.
/// </summary>
public class FsPromises
{
    public Task access(string path, int mode = 0) => fs.access(path, mode);
    public Task appendFile(string path, string data, string? encoding = "utf-8") => fs.appendFile(path, data, encoding);
    public Task chmod(string path, int mode) => fs.chmod(path, mode);
    public Task close(int fd) => fs.close(fd);
    public Task copyFile(string src, string dest, int mode = 0) => fs.copyFile(src, dest, mode);
    public Task cp(string src, string dest, bool recursive = false) => fs.cp(src, dest, recursive);
    public Task<Stats> fstat(int fd) => fs.fstat(fd);
    public Task mkdir(string path, bool recursive = false) => fs.mkdir(path, recursive);
    public Task mkdir(string path, MkdirOptions? options) => fs.mkdir(path, options);
    public Task mkdir(string path, object? options) => fs.mkdir(path, options);
    public Task<int> open(string path, string flags, int? mode = null) => fs.open(path, flags, mode);
    public Task<int> read(int fd, byte[] buffer, int offset, int length, int? position) => fs.read(fd, buffer, offset, length, position);
    public Task<string[]> readdir(string path) => fs.readdir(path);
    public Task<object[]> readdir(string path, bool withFileTypes) => fs.readdir(path, withFileTypes);
    public Task<Dirent[]> readdirDirents(string path) => fs.readdirDirents(path);
    public Task<string> readFile(string path, string encoding = "utf-8") => fs.readFile(path, encoding);
    public Task<byte[]> readFileBytes(string path) => fs.readFileBytes(path);
    public Task<string> readlink(string path) => fs.readlink(path);
    public Task<string> realpath(string path) => fs.realpath(path);
    public Task rename(string oldPath, string newPath) => fs.rename(oldPath, newPath);
    public Task rm(string path, bool recursive = false) => fs.rm(path, recursive);
    public Task rmdir(string path, bool recursive = false) => fs.rmdir(path, recursive);
    public Task<Stats> stat(string path) => fs.stat(path);
    public Task symlink(string target, string path, string? type = null) => fs.symlink(target, path, type);
    public Task truncate(string path, long len = 0) => fs.truncate(path, len);
    public Task unlink(string path) => fs.unlink(path);
    public Task<int> write(int fd, byte[] buffer, int offset, int length, int? position) => fs.write(fd, buffer, offset, length, position);
    public Task<int> write(int fd, string data, int? position = null, string? encoding = null) => fs.write(fd, data, position, encoding);
    public Task writeFile(string path, string data, string? encoding = "utf-8") => fs.writeFile(path, data, encoding);
    public Task writeFileBytes(string path, byte[] data) => fs.writeFileBytes(path, data);
}

#pragma warning restore CS1591
