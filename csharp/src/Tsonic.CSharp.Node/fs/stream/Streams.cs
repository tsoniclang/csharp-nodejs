using System.Text;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

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
            var effectiveStart = options.start ?? 0;
            if (end < effectiveStart)
                throw new ArgumentOutOfRangeException(nameof(options), "ReadStream end must not precede start.");
            _remaining = checked(end - effectiveStart + 1);
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
        var requested = System.Math.Max(1, System.Math.Min(size, _highWaterMark));
        if (_remaining is long remaining)
            requested = checked((int)System.Math.Min(requested, remaining));
        var bytes = new byte[requested];
        var count = _stream.Read(bytes, 0, bytes.Length);
        if (count == 0)
        {
            _stream.Dispose();
            push(null);
            return;
        }
        if (count != bytes.Length)
            System.Array.Resize(ref bytes, count);
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
