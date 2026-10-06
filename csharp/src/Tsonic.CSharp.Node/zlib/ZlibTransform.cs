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
        _ = BackgroundDispatch.RunReferenced(Process);
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

    private void Process()
    {
        try
        {
            using var output = new CodecOutputStream(
                PublishOutput,
                _zlibOptions?.maxOutputLength ?? _brotliOptions?.maxOutputLength);
            RunCodec(_input, output);
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

    private void PublishOutput(byte[] bytes)
    {
        _stopping.Token.ThrowIfCancellationRequested();
        var completed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() =>
        {
            if (_stopping.IsCancellationRequested)
            {
                completed.TrySetCanceled(_stopping.Token);
                return;
            }
            try
            {
                completed.TrySetResult(push(Buffer.TakeOwnership(bytes)));
            }
            catch (Exception error)
            {
                if (destroyed)
                {
                    completed.TrySetCanceled(_stopping.Token);
                    throw;
                }
                try
                {
                    destroy(error);
                }
                finally
                {
                    completed.TrySetCanceled(_stopping.Token);
                }
            }
        });
        var accepted = completed.Task.WaitAsync(_stopping.Token).GetAwaiter().GetResult();
        if (!accepted)
            WaitForReadCapacity(_stopping.Token);
    }

    private void RunCodec(System.IO.Stream input, System.IO.Stream output)
    {
        var bufferSize = _zlibOptions?.chunkSize ?? _brotliOptions?.chunkSize ?? 16 * 1024;
        switch (_mode)
        {
            case ZlibMode.Gzip:
                CopyIntoCompressor(input, output, new GZipStream(output, CompressionLevelFor(_zlibOptions?.level), true), bufferSize);
                return;
            case ZlibMode.Deflate:
                CopyIntoCompressor(input, output, new ZLibStream(output, CompressionLevelFor(_zlibOptions?.level), true), bufferSize);
                return;
            case ZlibMode.DeflateRaw:
                CopyIntoCompressor(input, output, new DeflateStream(output, CompressionLevelFor(_zlibOptions?.level), true), bufferSize);
                return;
            case ZlibMode.BrotliCompress:
                CopyIntoCompressor(input, output, new BrotliStream(output, CompressionLevelForBrotli(_brotliOptions?.quality), true), bufferSize);
                return;
            case ZlibMode.Gunzip:
                CopyFromDecompressor(new GZipStream(input, CompressionMode.Decompress, true), output, bufferSize);
                return;
            case ZlibMode.Inflate:
                CopyFromDecompressor(new ZLibStream(input, CompressionMode.Decompress, true), output, bufferSize);
                return;
            case ZlibMode.InflateRaw:
                CopyFromDecompressor(new DeflateStream(input, CompressionMode.Decompress, true), output, bufferSize);
                return;
            case ZlibMode.BrotliDecompress:
                CopyFromDecompressor(new BrotliStream(input, CompressionMode.Decompress, true), output, bufferSize);
                return;
            case ZlibMode.Unzip:
                CopyUnzip(input, output, bufferSize);
                return;
            default:
                throw new InvalidOperationException($"Unsupported zlib mode '{_mode}'.");
        }
    }

    private static void CopyIntoCompressor(
        System.IO.Stream input,
        System.IO.Stream output,
        System.IO.Stream compressor,
        int bufferSize)
    {
        using (compressor)
            input.CopyTo(compressor, bufferSize);
        output.Flush();
    }

    private static void CopyFromDecompressor(
        System.IO.Stream decompressor,
        System.IO.Stream output,
        int bufferSize)
    {
        using (decompressor)
            decompressor.CopyTo(output, bufferSize);
    }

    private static void CopyUnzip(
        System.IO.Stream input,
        System.IO.Stream output,
        int bufferSize)
    {
        var prefix = new byte[2];
        var count = 0;
        while (count < prefix.Length)
        {
            var read = input.Read(prefix, count, prefix.Length - count);
            if (read == 0)
                throw new InvalidDataException("Compressed input is too short to identify its format.");
            count += read;
        }
        using var prefixed = new PrefixStream(prefix, input);
        if (prefix[0] == 0x1f && prefix[1] == 0x8b)
            CopyFromDecompressor(new GZipStream(prefixed, CompressionMode.Decompress, true), output, bufferSize);
        else
            CopyFromDecompressor(new ZLibStream(prefixed, CompressionMode.Decompress, true), output, bufferSize);
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
                        Monitor.PulseAll(_sync);
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
                Monitor.PulseAll(_sync);
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
                Monitor.PulseAll(_sync);
            }
            foreach (var chunk in pending)
                Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => chunk.Consumed(error));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            var destination = buffer.AsSpan(offset, count);
            if (count == 0)
                return 0;

            Action<Exception?>? consumed = null;
            int copied;
            lock (_sync)
            {
                while (_current is null && _chunks.Count == 0 && !_completed)
                    Monitor.Wait(_sync);
                if (_failure is not null)
                    throw _failure;
                if (_current is null)
                {
                    if (!_chunks.TryDequeue(out _current))
                        return 0;
                    _offset = 0;
                }

                var current = _current;
                copied = Math.Min(count, current.Data.Length - _offset);
                current.Data.Span.Slice(_offset, copied).CopyTo(destination);
                _offset += copied;
                if (_offset == current.Data.Length)
                {
                    _current = null;
                    consumed = current.Consumed;
                }
            }
            if (consumed is not null)
                Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => consumed(null));
            return copied;
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
        private readonly Action<byte[]> _publish;
        private readonly long? _maximumLength;
        private long _length;

        public CodecOutputStream(Action<byte[]> publish, int? maximumLength)
        {
            _publish = publish;
            _maximumLength = maximumLength;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            var source = buffer.AsSpan(offset, count);
            if (count == 0)
                return;
            var nextLength = checked(_length + count);
            if (_maximumLength is long maximum && nextLength > maximum)
                throw new InvalidDataException("Codec output exceeds maxOutputLength.");
            var bytes = new byte[count];
            source.CopyTo(bytes);
            _publish(bytes);
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

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_offset < _prefix.Length)
            {
                var copied = Math.Min(count, _prefix.Length - _offset);
                Array.Copy(_prefix, _offset, buffer, offset, copied);
                _offset += copied;
                return copied;
            }
            return _source.Read(buffer, offset, count);
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
