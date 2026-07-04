using System.Text;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public sealed class FileHandle
{
    public FileHandle(int fd)
    {
        this.fd = fd;
    }

    public int fd { get; }
    public Task sync() => fs.fsync(fd);
    public Task datasync() => fs.fdatasync(fd);
    public Task chown(int uid, int gid) => fs.fchown(fd, uid, gid);
    public Task chmod(int mode) => fs.fchmod(fd, mode);
    public Task truncate(long len = 0) => fs.ftruncate(fd, len);
    public Task<Stats> stat() => fs.fstat(fd);
    public Task close() => fs.close(fd);
    public Task<int> read(byte[] buffer, int offset, int length, int? position = null) => fs.read(fd, buffer, offset, length, position);
    public Task<int> write(byte[] buffer, int offset, int length, int? position = null) => fs.write(fd, buffer, offset, length, position);
    public Task<int> write(string data, int? position = null, string? encoding = null) => fs.write(fd, data, position, encoding);
    public Task<ReadVResult> readv(byte[][] buffers, long? position = null) => Task.FromResult(fs.readvSync(fd, buffers, position));
    public Task<WriteVResult> writev(byte[][] buffers, long? position = null) => Task.FromResult(fs.writevSync(fd, buffers, position));
    public ReadStream createReadStream() => new(FileDescriptorManager.Get(fd)?.Name ?? throw new ArgumentException($"Bad file descriptor: {fd}", nameof(fd)));
    public WriteStream createWriteStream() => new(FileDescriptorManager.Get(fd)?.Name ?? throw new ArgumentException($"Bad file descriptor: {fd}", nameof(fd)));
    public async Task<string[]> readLines(string encoding = "utf-8")
    {
        var stream = FileDescriptorManager.Get(fd) ?? throw new ArgumentException($"Bad file descriptor: {fd}", nameof(fd));
        var text = await File.ReadAllTextAsync(stream.Name, Encoding.GetEncoding(encoding)).ConfigureAwait(false);
        return text.Split('\n');
    }
}

public sealed class FileHandleWriter
{
    private readonly FileHandle _handle;

    public FileHandleWriter(FileHandle handle)
    {
        _handle = handle;
    }

    public long position { get; private set; }
    public void seek(long position) => this.position = position;
    public async Task<int> write(string value)
    {
        var written = await _handle.write(value, (int)position).ConfigureAwait(false);
        position += written;
        return written;
    }

    public async Task<int> write(Buffer value)
    {
        var bytes = new byte[value.length];
        for (var i = 0; i < value.length; i++)
            bytes[i] = value[i];
        var written = await _handle.write(bytes, 0, bytes.Length, (int)position).ConfigureAwait(false);
        position += written;
        return written;
    }
}
