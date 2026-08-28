using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public class ZlibTransform : Transform
{
    private readonly CodecInputStream _input = new();
    private readonly ZlibMode _mode;
    private readonly ZlibOptions? _zlibOptions;
    private readonly BrotliOptions? _brotliOptions;
    private readonly Task _processor;
    private Exception? _processorError;
    private int _flushStarted;

    public ZlibTransform(
        ZlibMode mode,
        ZlibOptions? zlibOptions = null,
        BrotliOptions? brotliOptions = null)
    {
        ValidateOptions(mode, zlibOptions, brotliOptions);
        _mode = mode;
        _zlibOptions = zlibOptions;
        _brotliOptions = brotliOptions;
        _processor = BackgroundDispatch.RunReferenced(Process);
    }

    public ZlibMode mode => _mode;

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

        var bytes = chunk switch
        {
            Buffer buffer => buffer.InternalData,
            byte[] value => value,
            _ => throw new ArgumentException("Zlib transforms accept Buffer or byte[] chunks.", nameof(chunk)),
        };
        _input.Enqueue(bytes, error => callback(error, null));
    }

    protected override void _flush(Action<Exception?> callback)
    {
        if (Interlocked.Exchange(ref _flushStarted, 1) != 0)
            throw new InvalidOperationException("Zlib transform was finalized more than once.");

        _input.Complete();
        _ = _processor.ContinueWith(
            _ => Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => callback(Volatile.Read(ref _processorError))),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
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
            Volatile.Write(ref _processorError, error);
            _input.Fail(error);
        }
    }

    private void PublishOutput(byte[] bytes)
    {
        Exception? failure = null;
        var accepted = false;
        var completed = new ManualResetEventSlim(false);
        Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() =>
        {
            try
            {
                accepted = push(Buffer.from(bytes));
            }
            catch (Exception error)
            {
                failure = error;
            }
            finally
            {
                completed.Set();
            }
        });
        completed.Wait();
        if (failure is not null)
            throw failure;
        if (!accepted)
            WaitForReadCapacity();
    }

    private void RunCodec(Stream input, Stream output)
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
        Stream input,
        Stream output,
        Stream compressor,
        int bufferSize)
    {
        using (compressor)
            input.CopyTo(compressor, bufferSize);
        output.Flush();
    }

    private static void CopyFromDecompressor(Stream decompressor, Stream output, int bufferSize)
    {
        using (decompressor)
            decompressor.CopyTo(output, bufferSize);
    }

    private static void CopyUnzip(Stream input, Stream output, int bufferSize)
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

    private sealed class CodecInputStream : Stream
    {
        private sealed record Chunk(byte[]? Data, Action<Exception?>? Consumed);

        private readonly BlockingCollection<Chunk> _chunks = new();
        private Chunk? _current;
        private int _offset;
        private int _completed;

        public void Enqueue(byte[] bytes, Action<Exception?> consumed)
        {
            if (Volatile.Read(ref _completed) != 0)
                throw new InvalidOperationException("Cannot write after the codec input has completed.");
            _chunks.Add(new Chunk(bytes, consumed));
        }

        public void Complete()
        {
            if (Interlocked.Exchange(ref _completed, 1) == 0)
                _chunks.Add(new Chunk(null, null));
        }

        public void Fail(Exception error)
        {
            Interlocked.Exchange(ref _completed, 1);
            var current = Interlocked.Exchange(ref _current, null);
            if (current?.Consumed is not null)
                Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => current.Consumed(error));
            while (_chunks.TryTake(out var chunk))
                if (chunk.Consumed is not null)
                    Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => chunk.Consumed(error));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            while (_current is null)
            {
                var next = _chunks.Take();
                if (next.Data is null)
                    return 0;
                _current = next;
                _offset = 0;
            }

            var current = _current;
            var available = current.Data!.Length - _offset;
            var copied = Math.Min(count, available);
            Array.Copy(current.Data, _offset, buffer, offset, copied);
            _offset += copied;
            if (_offset == current.Data.Length)
            {
                _current = null;
                if (current.Consumed is not null)
                    Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => current.Consumed(null));
            }
            return copied;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() { }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private sealed class CodecOutputStream : Stream
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
            var nextLength = checked(_length + count);
            if (_maximumLength is long maximum && nextLength > maximum)
                throw new InvalidDataException("Codec output exceeds maxOutputLength.");
            var bytes = new byte[count];
            Array.Copy(buffer, offset, bytes, 0, count);
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
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
    }

    private sealed class PrefixStream : Stream
    {
        private readonly byte[] _prefix;
        private readonly Stream _source;
        private int _offset;

        public PrefixStream(byte[] prefix, Stream source)
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
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }
}
