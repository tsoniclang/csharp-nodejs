using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

/// <summary>
/// A readable stream is an abstraction for a source from which data is read.
/// </summary>
public partial class Readable : Stream
{
    private readonly LinkedList<object?> _buffer = new();
    private readonly object _readLock = new();
    private TaskCompletionSource? _capacityWaiter;
    private readonly int _highWaterMark;
    private long _bufferedSize;
    private bool _ended = false;
    private bool _flowing = false;
    private string? _encoding;
    private bool _paused = true;
    private bool _reading;
    private bool _endEmitted;

    /// <summary>Creates a readable stream with the default finite buffer limit.</summary>
    public Readable()
        : this(64 * 1024)
    {
    }

    /// <summary>Creates a readable stream with the selected finite buffer limit.</summary>
    protected Readable(int highWaterMark)
    {
        if (highWaterMark <= 0)
            throw new ArgumentOutOfRangeException(nameof(highWaterMark));
        _highWaterMark = highWaterMark;
    }

    /// <summary>Creates a finite in-memory binary readable without copying its chunks.</summary>
    public static Readable from(Buffer[] chunks)
    {
        ArgumentNullException.ThrowIfNull(chunks);
        var readable = new Readable();
        foreach (var chunk in chunks)
            readable.push(chunk ?? throw new ArgumentException("Readable chunks cannot contain null.", nameof(chunks)));
        readable.push(null);
        return readable;
    }

    /// <summary>
    /// Is true if it is safe to call read().
    /// </summary>
    public bool readable { get { lock (_readLock) return !_ended && !_destroyed; } }

    /// <summary>
    /// Becomes true when 'end' event is emitted.
    /// </summary>
    public bool readableEnded { get { lock (_readLock) return _ended; } }

    /// <summary>
    /// This property reflects the current state of a Readable stream.
    /// </summary>
    public bool? readableFlowing { get { lock (_readLock) return _flowing ? true : (_paused ? false : null); } }

    /// <summary>
    /// This property contains the number of bytes (or objects) in the queue ready to be read.
    /// </summary>
    public int readableLength { get { lock (_readLock) return checked((int)Math.Min(_bufferedSize, int.MaxValue)); } }

    /// <summary>
    /// Is true after destroy() has been called.
    /// </summary>
    private bool _destroyed;
    /// <summary>Indicates whether the stream has been destroyed.</summary>
    public bool destroyed { get { lock (_readLock) return _destroyed; } }

    /// <summary>
    /// Reads data out of the internal buffer and returns it.
    /// </summary>
    /// <param name="size">Optional argument to specify how much data to read.</param>
    /// <returns>The data read, or null if no data is available.</returns>
    public object? read(int? size = null)
    {
        if (size is <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Read size must be positive.");
        var requestRead = false;
        lock (_readLock)
        {
            if (_buffer.Count == 0 && !_ended && !_reading)
            {
                _reading = true;
                requestRead = true;
            }
        }
        if (requestRead)
            _read(size ?? 64 * 1024);

        object? chunk;
        var emitEnd = false;
        lock (_readLock)
        {
            if (_buffer.Count == 0)
            {
                emitEnd = MarkEndReady();
                chunk = null;
            }
            else
            {
                var first = _buffer.First!;
                if (size is int requested && first.Value is Buffer buffer && requested < buffer.length)
                {
                    chunk = buffer.subarray(0, requested);
                    first.Value = buffer.subarray(requested);
                    _bufferedSize = checked(_bufferedSize - requested);
                }
                else
                {
                    chunk = first.Value;
                    _buffer.RemoveFirst();
                    _bufferedSize = checked(_bufferedSize - ChunkSize(chunk));
                }
                SignalReadCapacityIfAvailable();
                emitEnd = MarkEndReady();
            }
        }
        if (emitEnd)
            emit("end");
        return chunk;
    }

    /// <summary>Reads a chunk as a closed TypeScript value.</summary>
    public TsValue readValue(int? size = null)
    {
        var value = read(size);
        return value is null ? TsValue.undefined() : TsValue.from(value);
    }

    /// <summary>Reads one binary chunk, or native absence at the current boundary.</summary>
    public Buffer? readBuffer(int? size = null)
    {
        return read(size) switch
        {
            null => null,
            Buffer buffer => buffer,
            byte[] bytes => Buffer.TakeOwnership(bytes),
            _ => throw new InvalidOperationException("The selected readable is not in binary mode."),
        };
    }

    /// <summary>
    /// Sets the character encoding for data read from the Readable stream.
    /// </summary>
    /// <param name="encoding">The encoding to use.</param>
    /// <returns>This stream.</returns>
    public Readable setEncoding(string encoding)
    {
        _ = Buffer.from(string.Empty, encoding);
        lock (_readLock)
            _encoding = encoding;
        return this;
    }

    /// <summary>
    /// Causes a stream in flowing mode to stop emitting 'data' events, switching out of flowing mode.
    /// </summary>
    /// <returns>This stream.</returns>
    public Readable pause()
    {
        lock (_readLock)
        {
            _paused = true;
            _flowing = false;
        }
        return this;
    }

    /// <summary>
    /// Causes an explicitly paused Readable stream to resume emitting 'data' events, switching the stream into flowing mode.
    /// </summary>
    /// <returns>This stream.</returns>
    public Readable resume()
    {
        lock (_readLock)
        {
            _paused = false;
            _flowing = true;
        }

        while (true)
        {
            lock (_readLock)
            {
                if (_paused || _destroyed)
                    break;
            }
            var chunk = read();
            if (chunk == null)
                break;
            emit("data", chunk);
        }

        EmitEndIfReady();

        return this;
    }

    /// <summary>
    /// Returns the current operating state of the Readable.
    /// </summary>
    /// <returns>True if the stream is paused.</returns>
    public bool isPaused()
    {
        lock (_readLock)
            return _paused;
    }

    /// <summary>
    /// Detaches a Writable stream previously attached using pipe().
    /// </summary>
    /// <param name="destination">Optional specific stream to unpipe.</param>
    /// <returns>This stream.</returns>
    public Readable unpipe(Stream? destination = null)
    {
        // Simplified implementation - would need to track pipes in full implementation
        return this;
    }

    /// <summary>Pipes this stream into the selected writable destination.</summary>
    public TDestination pipeTo<TDestination>(TDestination destination)
        where TDestination : Writable
    {
        _ = pipe(destination);
        return destination;
    }

    /// <summary>
    /// Pushes a chunk of data back into the internal buffer.
    /// </summary>
    /// <param name="chunk">Chunk of data to unshift onto the read queue.</param>
    public void unshift(object? chunk)
    {
        if (chunk == null)
            return;
        lock (_readLock)
        {
            _buffer.AddFirst(chunk);
            _bufferedSize = checked(_bufferedSize + ChunkSize(chunk));
            BlockReadCapacityIfNeeded();
        }
    }

    /// <summary>
    /// Pushes a chunk of data into the internal buffer. Can be called by subclasses.
    /// </summary>
    /// <param name="chunk">Chunk of data to push.</param>
    /// <param name="encoding">Optional encoding for string chunks.</param>
    /// <returns>True if the internal buffer has not exceeded highWaterMark.</returns>
    public bool push(object? chunk, string? encoding = null)
    {
        var emitReadable = false;
        var emitEnd = false;
        List<object?>? flowingChunks = null;
        var accepted = false;
        lock (_readLock)
        {
            if (chunk == null)
            {
                _reading = false;
                _ended = true;
                emitEnd = MarkEndReady();
            }
            else
            {
                _reading = false;
                var normalizedChunk = _encoding == null
                    ? chunk
                    : chunk switch
                    {
                        Buffer buffer => buffer.toString(_encoding),
                        byte[] bytes => Buffer.from(bytes).toString(_encoding),
                        _ => chunk,
                    };
                _buffer.AddLast(normalizedChunk);
                _bufferedSize = checked(_bufferedSize + ChunkSize(normalizedChunk));
                if (_flowing)
                {
                    flowingChunks = new List<object?>(_buffer.Count);
                    while (_buffer.Count > 0)
                    {
                        var buffered = _buffer.First!.Value;
                        _buffer.RemoveFirst();
                        flowingChunks.Add(buffered);
                        _bufferedSize = checked(_bufferedSize - ChunkSize(buffered));
                    }
                    SignalReadCapacityIfAvailable();
                    emitEnd = MarkEndReady();
                }
                else
                {
                    BlockReadCapacityIfNeeded();
                    emitReadable = true;
                }
                accepted = _bufferedSize < _highWaterMark;
            }
        }

        if (flowingChunks != null)
            foreach (var data in flowingChunks)
                emit("data", data);
        if (emitReadable)
            emit("readable");
        if (emitEnd)
            emit("end");

        return chunk != null && accepted;
    }

    /// <summary>
    /// Destroys the stream.
    /// </summary>
    /// <param name="error">Optional error to emit.</param>
    public override void destroy(Exception? error = null)
    {
        lock (_readLock)
        {
            if (_destroyed)
                return;
            _destroyed = true;
            _buffer.Clear();
            _bufferedSize = 0;
            SignalReadCapacityIfAvailable();
        }

        base.destroy(error);
    }

    /// <summary>
    /// Internal method to be implemented by subclasses to read data.
    /// </summary>
    /// <param name="size">Number of bytes to read.</param>
    protected virtual void _read(int size)
    {
        // To be implemented by subclasses
    }

    /// <inheritdoc />
    protected override void OnListenerAdded(object eventName)
    {
        if (eventName is string text && string.Equals(text, "data", StringComparison.Ordinal))
            resume();
    }

    /// <summary>Waits until the finite readable buffer can accept more data.</summary>
    protected void WaitForReadCapacity(CancellationToken cancellationToken = default)
    {
        CapacityTask().WaitAsync(cancellationToken).GetAwaiter().GetResult();
    }

    /// <summary>Asynchronously waits until the finite readable buffer can accept more data.</summary>
    protected Task WaitForReadCapacityAsync(CancellationToken cancellationToken = default) =>
        CapacityTask().WaitAsync(cancellationToken);

    /// <summary>Destroys the stream and returns it for the source-level fluent contract.</summary>
    public Readable destroyChain(Exception? error = null)
    {
        destroy(error);
        return this;
    }

    private void EmitEndIfReady()
    {
        bool emitEnd;
        lock (_readLock)
            emitEnd = MarkEndReady();
        if (emitEnd)
            emit("end");
    }

    private bool MarkEndReady()
    {
        if (!_ended || _buffer.Count != 0 || _endEmitted)
            return false;
        _endEmitted = true;
        return true;
    }

    private Task CapacityTask()
    {
        lock (_readLock)
        {
            if (_destroyed || _bufferedSize < _highWaterMark)
                return Task.CompletedTask;
            _capacityWaiter ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            return _capacityWaiter.Task;
        }
    }

    private void BlockReadCapacityIfNeeded()
    {
        if (_bufferedSize < _highWaterMark || _destroyed)
            return;
        _capacityWaiter ??= new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private void SignalReadCapacityIfAvailable()
    {
        if (!_destroyed && _bufferedSize >= _highWaterMark)
            return;
        var waiter = _capacityWaiter;
        _capacityWaiter = null;
        waiter?.TrySetResult();
    }

    private static int ChunkSize(object? chunk)
    {
        return chunk switch
        {
            null => 0,
            Buffer buffer => buffer.length,
            byte[] bytes => bytes.Length,
            string text => Encoding.UTF8.GetByteCount(text),
            _ => 1,
        };
    }
}
