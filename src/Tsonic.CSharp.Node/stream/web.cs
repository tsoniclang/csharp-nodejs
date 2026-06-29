#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public sealed class StreamPipeOptions
{
    public bool preventClose { get; set; }

    public bool preventAbort { get; set; }

    public bool preventCancel { get; set; }

    public CancellationToken signal { get; set; }
}

public sealed class ReadableStreamGetReaderOptions
{
    public string? mode { get; set; }
}

public sealed class ReadableStreamIteratorOptions
{
    public bool preventCancel { get; set; }
}

public sealed class ReadableStreamBYOBReaderReadOptions
{
    public int? min { get; set; }
}

public sealed class ReadableStreamReadResult
{
    public object? value { get; set; }

    public bool done { get; set; }
}

public sealed class ReadableStream
{
    private readonly Queue<object?> _chunks;
    private bool _locked;
    private bool _canceled;

    public ReadableStream()
    {
        _chunks = [];
    }

    private ReadableStream(IEnumerable<object?> chunks)
    {
        _chunks = new Queue<object?>(chunks);
    }

    public static ReadableStream fromChunks(params object?[] chunks)
    {
        return new ReadableStream(chunks);
    }

    public object?[] chunks => _chunks.ToArray();

    public bool locked => _locked;

    public bool canceled => _canceled;

    public ReadableStreamDefaultReader getReader(ReadableStreamGetReaderOptions? options = null)
    {
        _ = options;
        if (_locked)
            throw new InvalidOperationException("ReadableStream is already locked.");

        _locked = true;
        return new ReadableStreamDefaultReader(this);
    }

    public Task cancel(object? reason = null)
    {
        _ = reason;
        _canceled = true;
        _chunks.Clear();
        return Task.CompletedTask;
    }

    public IEnumerable<object?> values(ReadableStreamIteratorOptions? options = null)
    {
        _ = options;
        while (_chunks.Count > 0)
            yield return _chunks.Dequeue();
    }

    public async Task pipeTo(WritableStream destination, StreamPipeOptions? options = null)
    {
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        options?.signal.ThrowIfCancellationRequested();
        foreach (var chunk in values())
            await destination.write(chunk).ConfigureAwait(false);

        if (options?.preventClose != true)
            await destination.close().ConfigureAwait(false);
    }

    public ReadableStream[] tee()
    {
        var snapshot = _chunks.ToArray();
        return [new ReadableStream(snapshot), new ReadableStream(snapshot)];
    }

    internal ReadableStreamReadResult ReadOne()
    {
        if (_chunks.Count == 0)
            return new ReadableStreamReadResult { done = true };

        return new ReadableStreamReadResult { value = _chunks.Dequeue(), done = false };
    }

    internal void ReleaseLock()
    {
        _locked = false;
    }

    internal void Enqueue(object? chunk)
    {
        _chunks.Enqueue(chunk);
    }

    internal void Close()
    {
    }
}

public sealed class ReadableStreamDefaultReader
{
    private readonly ReadableStream _stream;
    private bool _released;

    internal ReadableStreamDefaultReader(ReadableStream stream)
    {
        _stream = stream;
    }

    public Task closed => Task.CompletedTask;

    public Task<ReadableStreamReadResult> read()
    {
        if (_released)
            throw new InvalidOperationException("Reader lock has been released.");

        return Task.FromResult(_stream.ReadOne());
    }

    public ReadableStreamReadResult readResult()
    {
        return read().GetAwaiter().GetResult();
    }

    public Task cancel(object? reason = null)
    {
        return _stream.cancel(reason);
    }

    public void releaseLock()
    {
        _released = true;
        _stream.ReleaseLock();
    }
}

public sealed class WritableStream
{
    private readonly List<object?> _chunks = [];
    private bool _locked;
    private bool _closed;
    private bool _aborted;

    public object?[] chunks => _chunks.ToArray();

    public bool locked => _locked;

    public bool closed => _closed;

    public bool aborted => _aborted;

    public Task write(object? chunk)
    {
        if (_closed)
            throw new InvalidOperationException("WritableStream is closed.");

        _chunks.Add(chunk);
        return Task.CompletedTask;
    }

    public Task close()
    {
        _closed = true;
        return Task.CompletedTask;
    }

    public Task abort(object? reason = null)
    {
        _ = reason;
        _aborted = true;
        _closed = true;
        return Task.CompletedTask;
    }

    public WritableStreamDefaultWriter getWriter()
    {
        if (_locked)
            throw new InvalidOperationException("WritableStream is already locked.");

        _locked = true;
        return new WritableStreamDefaultWriter(this);
    }

    internal void ReleaseLock()
    {
        _locked = false;
    }
}

public sealed class WritableStreamDefaultWriter
{
    private readonly WritableStream _stream;

    internal WritableStreamDefaultWriter(WritableStream stream)
    {
        _stream = stream;
    }

    public Task ready => Task.CompletedTask;

    public Task closed => _stream.closed ? Task.CompletedTask : Task.CompletedTask;

    public int? desiredSize => null;

    public Task write(object? chunk) => _stream.write(chunk);

    public Task close() => _stream.close();

    public Task abort(object? reason = null) => _stream.abort(reason);

    public void releaseLock() => _stream.ReleaseLock();
}

public sealed class TransformStream
{
    public TransformStream()
    {
        readable = new ReadableStream();
        writable = new WritableStream();
    }

    public ReadableStream readable { get; }

    public WritableStream writable { get; }

    public async Task writePassthrough(object? chunk)
    {
        await writable.write(chunk).ConfigureAwait(false);
        readable.Enqueue(chunk);
    }
}

public sealed class QueuingStrategyInit
{
    public double highWaterMark { get; set; } = 1;
}

public class QueuingStrategy
{
    public QueuingStrategy(double highWaterMark = 1)
    {
        this.highWaterMark = highWaterMark;
    }

    public double highWaterMark { get; }

    public virtual double size(object? chunk)
    {
        _ = chunk;
        return 1;
    }
}

public sealed class CountQueuingStrategy : QueuingStrategy
{
    public CountQueuingStrategy(QueuingStrategyInit init) : base(init.highWaterMark)
    {
    }
}

public sealed class ByteLengthQueuingStrategy : QueuingStrategy
{
    public ByteLengthQueuingStrategy(QueuingStrategyInit init) : base(init.highWaterMark)
    {
    }

    public override double size(object? chunk)
    {
        return chunk switch
        {
            byte[] bytes => bytes.Length,
            Buffer buffer => buffer.length,
            string text => text.Length,
            _ => 1
        };
    }
}

public sealed class ReadableStreamDefaultController
{
    private readonly ReadableStream _stream;

    public ReadableStreamDefaultController(ReadableStream stream)
    {
        _stream = stream;
    }

    public int? desiredSize => null;

    public void enqueue(object? chunk) => _stream.Enqueue(chunk);

    public void close() => _stream.Close();

    public void error(object? error) => throw new InvalidOperationException(error?.ToString());
}

public sealed class ReadableByteStreamController
{
    public ReadableByteStreamController(ReadableStream stream)
    {
        _stream = stream;
    }

    private readonly ReadableStream _stream;

    public int? desiredSize => null;

    public ReadableStreamBYOBRequest? byobRequest { get; private set; }

    public void setByobRequest(ReadableStreamBYOBRequest request) => byobRequest = request;

    public void enqueue(byte[] chunk) => _stream.Enqueue(chunk);

    public void close() => _stream.Close();

    public void error(object? error) => throw new InvalidOperationException(error?.ToString());
}

public sealed class ReadableStreamBYOBRequest
{
    public ReadableStreamBYOBRequest(byte[] view)
    {
        this.view = view;
    }

    public byte[] view { get; private set; }

    public int bytesWritten { get; private set; }

    public void respond(int bytesWritten)
    {
        this.bytesWritten = bytesWritten;
    }

    public void respondWithNewView(byte[] view)
    {
        this.view = view;
        bytesWritten = view.Length;
    }
}

public sealed class WritableStreamDefaultController
{
    public CancellationToken signal { get; set; }

    public bool abortSignal => signal.IsCancellationRequested;

    public bool errored { get; private set; }

    public void error(object? error)
    {
        _ = error;
        errored = true;
    }
}

public sealed class TransformStreamDefaultController
{
    private readonly List<object?> _chunks = [];

    public int? desiredSize => null;

    public object?[] chunks => _chunks.ToArray();

    public void enqueue(object? chunk) => _chunks.Add(chunk);

    public void error(object? error) => throw new InvalidOperationException(error?.ToString());

    public void terminate() => _chunks.Clear();
}

public sealed class GenericTransformStream
{
    public GenericTransformStream(ReadableStream readable, WritableStream writable)
    {
        this.readable = readable;
        this.writable = writable;
    }

    public ReadableStream readable { get; }

    public WritableStream writable { get; }
}

public sealed class ReadableWritablePair
{
    public ReadableWritablePair(ReadableStream readable, WritableStream writable)
    {
        this.readable = readable;
        this.writable = writable;
    }

    public ReadableStream readable { get; }

    public WritableStream writable { get; }
}

public sealed class TextEncoderStream
{
    public TextEncoderStream()
    {
        readable = new ReadableStream();
        writable = new WritableStream();
    }

    public ReadableStream readable { get; }

    public WritableStream writable { get; }
}

public sealed class TextDecoderStream
{
    public TextDecoderStream()
    {
        readable = new ReadableStream();
        writable = new WritableStream();
    }

    public ReadableStream readable { get; }

    public WritableStream writable { get; }
}

public sealed class CompressionStream
{
    public CompressionStream(string format)
    {
        this.format = format;
        readable = new ReadableStream();
        writable = new WritableStream();
    }

    public string format { get; }

    public ReadableStream readable { get; }

    public WritableStream writable { get; }
}

public sealed class DecompressionStream
{
    public DecompressionStream(string format)
    {
        this.format = format;
        readable = new ReadableStream();
        writable = new WritableStream();
    }

    public string format { get; }

    public ReadableStream readable { get; }

    public WritableStream writable { get; }
}

public partial class Readable
{
    public static ReadableStream toWeb(Readable readable)
    {
        if (readable == null)
            throw new ArgumentNullException(nameof(readable));

        var chunks = new List<object?>();
        object? chunk;
        while ((chunk = readable.read()) != null)
            chunks.Add(chunk);
        return ReadableStream.fromChunks(chunks.ToArray());
    }

    public static Readable fromWeb(ReadableStream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        var readable = new Readable();
        foreach (var chunk in stream.values())
            readable.push(chunk);
        readable.push(null);
        return readable;
    }
}

public partial class Writable
{
    public static WritableStream toWeb(Writable writable)
    {
        if (writable == null)
            throw new ArgumentNullException(nameof(writable));

        return new WritableStream();
    }

    public static Writable fromWeb(WritableStream stream)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));

        var writable = new Writable();
        foreach (var chunk in stream.chunks)
            writable.write(chunk);
        return writable;
    }
}
