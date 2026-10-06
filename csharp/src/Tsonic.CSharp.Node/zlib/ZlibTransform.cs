using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

/// <summary>Transforms stream chunks using the selected Node-compatible compression codec.</summary>
public class ZlibTransform : Transform
{
    private readonly CodecInputStream _input = new();
    private readonly ZlibMode _mode;
    private readonly ZlibOptions? _zlibOptions;
    private readonly BrotliOptions? _brotliOptions;
    private readonly object _completionSync = new();
    private readonly CancellationTokenSource _stopping = new();
    private Exception? _processorError;
    private Action<Exception?>? _flushCallback;
    private bool _processorCompleted;
    private bool _flushStarted;
    private int _destroyStarted;

    /// <summary>Creates a transform for the selected codec and validated options.</summary>
    public ZlibTransform(
        ZlibMode mode,
        ZlibOptions? zlibOptions = null,
        BrotliOptions? brotliOptions = null)
    {
        ValidateOptions(mode, zlibOptions, brotliOptions);
        _mode = mode;
        _zlibOptions = zlibOptions;
        _brotliOptions = brotliOptions;
        _ = BackgroundDispatch.RunReferencedAsync(Process);
    }

    /// <summary>Gets the codec mode selected for this transform.</summary>
    public ZlibMode mode => _mode;

    /// <inheritdoc />
    public override void destroy(Exception? error = null)
    {
        if (Interlocked.Exchange(ref _destroyStarted, 1) != 0)
            return;

        var failure = error ?? Volatile.Read(ref _processorError);
        _stopping.Cancel();
        _input.Fail(failure ?? new OperationCanceledException(_stopping.Token));
        base.destroy(failure);
    }

    /// <inheritdoc />
    protected override void _transform(
        object? chunk,
        string? encoding,
        Action<Exception?, object?> callback)
    {
        _ = encoding;
        var failure = Volatile.Read(ref _processorError);
        if (failure is not null)
        {
            callback(failure, null);
            return;
        }

        ReadOnlyMemory<byte> bytes = chunk switch
        {
            Buffer buffer => buffer.InternalMemory,
            byte[] value => value,
            _ => throw new ArgumentException("Zlib transforms accept Buffer or byte[] chunks.", nameof(chunk)),
        };
        _input.Enqueue(bytes, error => callback(error, null));
    }

    /// <inheritdoc />
    protected override void _flush(Action<Exception?> callback)
    {
        bool completed;
        lock (_completionSync)
        {
            if (_flushStarted)
                throw new InvalidOperationException("Zlib transform was finalized more than once.");
            _flushStarted = true;
            completed = _processorCompleted;
            if (!completed)
                _flushCallback = callback;
        }
        _input.Complete();
        if (completed)
            Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => callback(Volatile.Read(ref _processorError)));
    }

    private async Task Process()
    {
        try
        {
            using var publication = new CodecPublication(
                bytes => push(Buffer.TakeOwnership(bytes)),
                error =>
                {
                    if (destroyed)
                        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
                    destroy(error);
                },
                WaitForReadCapacityAsync,
                _stopping.Token);
            using var output = new CodecOutputStream(
                publication.Publish,
                _zlibOptions?.maxOutputLength ?? _brotliOptions?.maxOutputLength);
            await RunCodec(_input, output, _stopping.Token).ConfigureAwait(false);
        }
        catch (Exception error)
        {
            if (_stopping.IsCancellationRequested)
                return;

            var failure = Interlocked.CompareExchange(ref _processorError, error, null) ?? error;
            _input.Fail(failure);
            Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => destroy(failure));
        }
        finally
        {
            Action<Exception?>? callback;
            lock (_completionSync)
            {
                _processorCompleted = true;
                callback = _flushCallback;
                _flushCallback = null;
            }
            if (callback is not null)
                Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => callback(Volatile.Read(ref _processorError)));
        }
    }

    private Task RunCodec(System.IO.Stream input, System.IO.Stream output, CancellationToken cancellationToken)
    {
        var bufferSize = _zlibOptions?.chunkSize ?? _brotliOptions?.chunkSize ?? 16 * 1024;
        switch (_mode)
        {
            case ZlibMode.Gzip:
                return CopyIntoCompressor(input, output, new GZipStream(output, CompressionLevelFor(_zlibOptions?.level), true), bufferSize, cancellationToken);
            case ZlibMode.Deflate:
                return CopyIntoCompressor(input, output, new ZLibStream(output, CompressionLevelFor(_zlibOptions?.level), true), bufferSize, cancellationToken);
            case ZlibMode.DeflateRaw:
                return CopyIntoCompressor(input, output, new DeflateStream(output, CompressionLevelFor(_zlibOptions?.level), true), bufferSize, cancellationToken);
            case ZlibMode.BrotliCompress:
                return CopyIntoCompressor(input, output, new BrotliStream(output, CompressionLevelForBrotli(_brotliOptions?.quality), true), bufferSize, cancellationToken);
            case ZlibMode.Gunzip:
                return CopyFromDecompressor(new GZipStream(input, CompressionMode.Decompress, true), output, bufferSize, cancellationToken);
            case ZlibMode.Inflate:
                return CopyFromDecompressor(new ZLibStream(input, CompressionMode.Decompress, true), output, bufferSize, cancellationToken);
            case ZlibMode.InflateRaw:
                return CopyFromDecompressor(new DeflateStream(input, CompressionMode.Decompress, true), output, bufferSize, cancellationToken);
            case ZlibMode.BrotliDecompress:
                return CopyFromDecompressor(new BrotliStream(input, CompressionMode.Decompress, true), output, bufferSize, cancellationToken);
            case ZlibMode.Unzip:
                return CopyUnzip(input, output, bufferSize, cancellationToken);
            default:
                throw new InvalidOperationException($"Unsupported zlib mode '{_mode}'.");
        }
    }

    private static async Task CopyIntoCompressor(
        System.IO.Stream input,
        System.IO.Stream output,
        System.IO.Stream compressor,
        int bufferSize,
        CancellationToken cancellationToken)
    {
        await using (compressor.ConfigureAwait(false))
            await input.CopyToAsync(compressor, bufferSize, cancellationToken).ConfigureAwait(false);
        await output.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    private static async Task CopyFromDecompressor(
        System.IO.Stream decompressor,
        System.IO.Stream output,
        int bufferSize,
        CancellationToken cancellationToken)
    {
        await using (decompressor.ConfigureAwait(false))
            await decompressor.CopyToAsync(output, bufferSize, cancellationToken).ConfigureAwait(false);
    }

    private static async Task CopyUnzip(
        System.IO.Stream input,
        System.IO.Stream output,
        int bufferSize,
        CancellationToken cancellationToken)
    {
        var prefix = new byte[2];
        var count = 0;
        while (count < prefix.Length)
        {
            var read = await input.ReadAsync(prefix.AsMemory(count), cancellationToken).ConfigureAwait(false);
            if (read == 0)
                throw new InvalidDataException("Compressed input is too short to identify its format.");
            count += read;
        }
        using var prefixed = new PrefixStream(prefix, input);
        if (prefix[0] == 0x1f && prefix[1] == 0x8b)
            await CopyFromDecompressor(new GZipStream(prefixed, CompressionMode.Decompress, true), output, bufferSize, cancellationToken).ConfigureAwait(false);
        else
            await CopyFromDecompressor(new ZLibStream(prefixed, CompressionMode.Decompress, true), output, bufferSize, cancellationToken).ConfigureAwait(false);
    }

    private static CompressionLevel CompressionLevelFor(int? level)
    {
        return level switch
        {
            null or -1 or >= 6 and <= 9 => CompressionLevel.Optimal,
            0 => CompressionLevel.NoCompression,
            >= 1 and <= 5 => CompressionLevel.Fastest,
            _ => throw new ArgumentOutOfRangeException(nameof(level), "Zlib compression level must be -1 through 9."),
        };
    }

    private static CompressionLevel CompressionLevelForBrotli(int? quality)
    {
        return quality switch
        {
            null or >= 4 and <= 8 => CompressionLevel.Optimal,
            >= 0 and <= 3 => CompressionLevel.Fastest,
            >= 9 and <= 11 => CompressionLevel.SmallestSize,
            _ => throw new ArgumentOutOfRangeException(nameof(quality), "Brotli quality must be 0 through 11."),
        };
    }

    private static void ValidateOptions(
        ZlibMode mode,
        ZlibOptions? zlibOptions,
        BrotliOptions? brotliOptions)
    {
        var chunkSize = zlibOptions?.chunkSize ?? brotliOptions?.chunkSize;
        if (chunkSize is <= 0)
            throw new ArgumentOutOfRangeException(nameof(chunkSize), "Codec chunk size must be positive.");
        var maximum = zlibOptions?.maxOutputLength ?? brotliOptions?.maxOutputLength;
        if (maximum is <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximum), "Maximum codec output length must be positive.");
        _ = mode;
    }

    private sealed class CodecInputStream : System.IO.Stream
    {
        private sealed record Chunk(ReadOnlyMemory<byte> Data, Action<Exception?> Consumed);

        private readonly Queue<Chunk> _chunks = new();
        private readonly object _sync = new();
        private Chunk? _current;
        private int _offset;
        private bool _completed;
        private Exception? _failure;
        private TaskCompletionSource? _readWaiter;

        public void Enqueue(ReadOnlyMemory<byte> bytes, Action<Exception?> consumed)
        {
            Exception? failure;
            lock (_sync)
            {
                failure = _failure;
                if (failure is null)
                {
                    if (_completed)
                        throw new InvalidOperationException("Cannot write after the codec input has completed.");
                    if (!bytes.IsEmpty)
                    {
                        _chunks.Enqueue(new Chunk(bytes, consumed));
                        SignalReadiness();
                        return;
                    }
                }
            }
            Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => consumed(failure));
        }

        public void Complete()
        {
            lock (_sync)
            {
                _completed = true;
                SignalReadiness();
            }
        }

        public void Fail(Exception error)
        {
            List<Chunk> pending;
            lock (_sync)
            {
                if (_failure is not null)
                    return;
                _failure = error;
                _completed = true;
                pending = new List<Chunk>(_chunks.Count + (_current is null ? 0 : 1));
                if (_current is not null)
                {
                    pending.Add(_current);
                    _current = null;
                }
                while (_chunks.TryDequeue(out var chunk))
                    pending.Add(chunk);
                SignalReadiness();
            }
            foreach (var chunk in pending)
                Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => chunk.Consumed(error));
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            ReadAsync(buffer.AsMemory(offset, count)).GetAwaiter().GetResult();

        [System.Runtime.CompilerServices.AsyncMethodBuilder(typeof(System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder<>))]
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (buffer.IsEmpty)
                return 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Action<Exception?>? consumed = null;
                Task? readiness = null;
                var copied = 0;
                lock (_sync)
                {
                    if (_failure is not null)
                        throw _failure;
                    if (_current is null && _chunks.TryDequeue(out _current))
                        _offset = 0;
                    if (_current is not null)
                    {
                        copied = Math.Min(buffer.Length, _current.Data.Length - _offset);
                        _current.Data.Span.Slice(_offset, copied).CopyTo(buffer.Span);
                        _offset += copied;
                        if (_offset == _current.Data.Length)
                        {
                            consumed = _current.Consumed;
                            _current = null;
                        }
                    }
                    else if (_completed)
                    {
                        return 0;
                    }
                    else
                    {
                        _readWaiter ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                        readiness = _readWaiter.Task;
                    }
                }
                if (readiness is not null)
                {
                    await readiness.WaitAsync(cancellationToken).ConfigureAwait(false);
                    continue;
                }
                if (consumed is not null)
                    Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => consumed(null));
                return copied;
            }
        }

        private void SignalReadiness()
        {
            var waiter = _readWaiter;
            _readWaiter = null;
            waiter?.TrySetResult();
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long offset, System.IO.SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private sealed class CodecOutputStream : System.IO.Stream
    {
        private readonly Func<byte[], ValueTask> _publish;
        private readonly long? _maximumLength;
        private long _length;

        public CodecOutputStream(Func<byte[], ValueTask> publish, int? maximumLength)
        {
            _publish = publish;
            _maximumLength = maximumLength;
        }

        public override void Write(byte[] buffer, int offset, int count) =>
            WriteAsync(buffer.AsMemory(offset, count)).GetAwaiter().GetResult();

        [System.Runtime.CompilerServices.AsyncMethodBuilder(typeof(System.Runtime.CompilerServices.PoolingAsyncValueTaskMethodBuilder))]
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (buffer.IsEmpty)
                return;
            var nextLength = checked(_length + buffer.Length);
            if (_maximumLength is long maximum && nextLength > maximum)
                throw new InvalidDataException("Codec output exceeds maxOutputLength.");
            var bytes = buffer.ToArray();
            await _publish(bytes).ConfigureAwait(false);
            _length = nextLength;
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => _length;
        public override long Position { get => _length; set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, System.IO.SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }

    private sealed class PrefixStream : System.IO.Stream
    {
        private readonly byte[] _prefix;
        private readonly System.IO.Stream _source;
        private int _offset;

        public PrefixStream(byte[] prefix, System.IO.Stream source)
        {
            _prefix = prefix;
            _source = source;
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            ReadAsync(buffer.AsMemory(offset, count)).GetAwaiter().GetResult();

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_offset < _prefix.Length)
            {
                var copied = Math.Min(buffer.Length, _prefix.Length - _offset);
                _prefix.AsSpan(_offset, copied).CopyTo(buffer.Span);
                _offset += copied;
                return new ValueTask<int>(copied);
            }
            return _source.ReadAsync(buffer, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long offset, System.IO.SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
