using System.Text;
using Tsonic.CSharp.Js;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS1591

public sealed class ReadStream : Readable
{
    private LoopDispatch _ownerDispatch;
    private LoopRegistration _loopRegistration;
    private int _nativeClosed;
    private LoopDispatch StreamDispatch => BackgroundDispatch.Bind(ref _ownerDispatch, NodePhases.Background);

    private readonly FileStream _stream;
    private readonly CancellationTokenSource _cancellation = new();
    private readonly int _highWaterMark;
    private long? _remaining;

    public ReadStream(string path, ReadStreamOptions? options = null)
        : base(ValidateHighWaterMark(options?.highWaterMark ?? 64 * 1024), BackgroundDispatch.Capture(NodePhases.RuntimeTasks))
    {
        _ = StreamDispatch;
        this.path = path;
        pending = false;
        var open = FsStreamOpenOptions.ForRead(options?.flags ?? "r");
        _highWaterMark = options?.highWaterMark ?? 64 * 1024;
        _loopRegistration = StreamDispatch.Register(static state => ((ReadStream)state!).StopForLoop(), this);
        try
        {
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
        catch
        {
            _loopRegistration.Dispose();
            _stream?.Dispose();
            _cancellation.Dispose();
            throw;
        }
    }

    private static int ValidateHighWaterMark(int value) =>
        value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));

    public string path { get; }
    public bool pending { get; private set; }
    public long bytesRead { get; private set; }

    protected override void _read(int size)
    {
        _ = ReadNextAsync(size);
    }

    private async Task ReadNextAsync(int size)
    {
        StreamDispatch.Acquire();
        try
        {
            if (_remaining == 0)
            {
                await _stream.DisposeAsync().ConfigureAwait(false);
                BackgroundDispatch.Enqueue(StreamDispatch, () => push(null));
                return;
            }
            var requested = System.Math.Max(1, System.Math.Min(size, _highWaterMark));
            if (_remaining is long remaining)
                requested = checked((int)System.Math.Min(requested, remaining));
            var bytes = new byte[requested];
            var count = await _stream.ReadAsync(bytes.AsMemory(), _cancellation.Token).ConfigureAwait(false);
            if (count == 0)
            {
                await _stream.DisposeAsync().ConfigureAwait(false);
                BackgroundDispatch.Enqueue(StreamDispatch, () => push(null));
                return;
            }
            bytesRead += count;
            if (_remaining is not null)
                _remaining -= count;
            var chunk = Buffer.TakeOwnership(bytes.AsMemory(0, count));
            BackgroundDispatch.Enqueue(StreamDispatch, () => push(chunk));
        }
        catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
        {
        }
        catch (Exception error)
        {
            BackgroundDispatch.Enqueue(StreamDispatch, () => destroy(error));
        }
        finally
        {
            StreamDispatch.Release();
        }
    }

    public void close() => destroy();

    private void StopForLoop()
    {
        if (Interlocked.Exchange(ref _nativeClosed, 1) != 0) return;
        _cancellation.Cancel();
        _stream.Dispose();
        _cancellation.Dispose();
        _loopRegistration.Dispose();
    }

    public override void destroy(Exception? error = null)
    {
        if (!destroyed)
        {
            StopForLoop();
        }
        base.destroy(error);
    }
}

public sealed class WriteStream : Writable
{
    private LoopDispatch _ownerDispatch;
    private LoopRegistration _loopRegistration;
    private int _nativeClosed;
    private LoopDispatch StreamDispatch => BackgroundDispatch.Bind(ref _ownerDispatch, NodePhases.Background);

    private readonly FileStream _stream;
    private readonly CancellationTokenSource _cancellation = new();
    private readonly string _defaultEncoding;
    private readonly bool _flushToDisk;

    public WriteStream(string path, WriteStreamOptions? options = null)
        : base(ValidateHighWaterMark(options?.highWaterMark ?? 64 * 1024))
    {
        _ = StreamDispatch;
        this.path = path;
        pending = false;
        _defaultEncoding = options?.encoding ?? "utf-8";
        _ = Encoding.GetEncoding(_defaultEncoding);
        _flushToDisk = options?.flush == true;
        var open = FsStreamOpenOptions.ForWrite(options?.flags ?? "w");
        var mode = options?.mode;
        if (mode is < 0 or > 0xFFF)
            throw new ArgumentOutOfRangeException(nameof(options), "WriteStream mode must be a valid Unix permission mask.");
        var fileOptions = new FileStreamOptions
        {
            Mode = open.Mode,
            Access = open.Access,
            Share = FileShare.ReadWrite | FileShare.Delete,
            BufferSize = options?.highWaterMark ?? 64 * 1024,
            Options = open.Options,
        };
        if (mode.HasValue && !OperatingSystem.IsWindows())
            fileOptions.UnixCreateMode = (UnixFileMode)mode.Value;
        _loopRegistration = StreamDispatch.Register(static state => ((WriteStream)state!).StopForLoop(), this);
        try
        {
            _stream = new FileStream(path, fileOptions);
            if (open.Append)
                _stream.Seek(0, SeekOrigin.End);
            else if (options?.start is long start)
            {
                if (start < 0)
                    throw new ArgumentOutOfRangeException(nameof(options), "WriteStream start must be non-negative.");
                _stream.Seek(start, SeekOrigin.Begin);
            }
        }
        catch
        {
            _loopRegistration.Dispose();
            _stream?.Dispose();
            _cancellation.Dispose();
            throw;
        }
    }

    private static int ValidateHighWaterMark(int value) =>
        value > 0 ? value : throw new ArgumentOutOfRangeException(nameof(value));

    public string path { get; }
    public bool pending { get; private set; }
    public long bytesWritten { get; private set; }

    public void close()
    {
        if (writable)
            end();
    }

    protected override void _write(object? chunk, string? encoding, Action callback)
    {
        ReadOnlyMemory<byte> bytes = chunk switch
        {
            Buffer buffer => buffer.InternalMemory,
            byte[] value => value,
            string value => Encoding.GetEncoding(encoding ?? _defaultEncoding).GetBytes(value),
            _ => throw new ArgumentException("WriteStream accepts Buffer, byte[], or string chunks.", nameof(chunk))
        };
        _ = WriteChunkAsync(bytes, callback);
    }

    protected override void _final(Action callback)
    {
        _ = FinalizeAsync(callback);
    }

    private void StopForLoop()
    {
        if (Interlocked.Exchange(ref _nativeClosed, 1) != 0) return;
        _cancellation.Cancel();
        _stream.Dispose();
        _cancellation.Dispose();
        _loopRegistration.Dispose();
    }

    public override void destroy(Exception? error = null)
    {
        if (!destroyed)
        {
            StopForLoop();
        }
        base.destroy(error);
    }

    private async Task WriteChunkAsync(ReadOnlyMemory<byte> bytes, Action callback)
    {
        StreamDispatch.Acquire();
        try
        {
            await _stream.WriteAsync(bytes, _cancellation.Token).ConfigureAwait(false);
            bytesWritten += bytes.Length;
            BackgroundDispatch.Enqueue(StreamDispatch, callback);
        }
        catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
        {
        }
        catch (Exception error)
        {
            BackgroundDispatch.Enqueue(StreamDispatch, () =>
            {
                destroy(error);
                callback();
            });
        }
        finally
        {
            StreamDispatch.Release();
        }
    }

    private async Task FinalizeAsync(Action callback)
    {
        StreamDispatch.Acquire();
        try
        {
            await _stream.FlushAsync(_cancellation.Token).ConfigureAwait(false);
            if (_flushToDisk)
                _stream.Flush(true);
            await _stream.DisposeAsync().ConfigureAwait(false);
            BackgroundDispatch.Enqueue(StreamDispatch, () =>
            {
                callback();
                emit("close");
            });
        }
        catch (OperationCanceledException) when (_cancellation.IsCancellationRequested)
        {
        }
        catch (Exception error)
        {
            BackgroundDispatch.Enqueue(StreamDispatch, () =>
            {
                destroy(error);
                callback();
            });
        }
        finally
        {
            StreamDispatch.Release();
        }
    }
}
