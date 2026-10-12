using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

/// <summary>
/// A readable stream is an abstraction for a source from which data is read.
/// </summary>
public partial class Readable : Stream
{
    private readonly ReadableChunkQueue _buffer = new();
    private readonly object _readLock = new();
    private readonly int _highWaterMark;
    private long _bufferedSize;
    private bool _ended = false;
    private bool _flowing = false;
    private string? _encoding;
    private bool _paused = true;
    private bool _reading;
    private bool _endEmitted;
    private bool _endScheduled;
    private bool _didRead;
    private bool _flowScheduled;
    private bool _flowActive;
    private bool _flowRequested;
    private Action? _flowDispatch;

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
    public static Readable from(IReadOnlyList<Buffer> chunks)
    {
        ArgumentNullException.ThrowIfNull(chunks);
        var readable = new Readable();
        for (var index = 0; index < chunks.Count; index++)
            readable.push(chunks[index] ?? throw new ArgumentException("Readable chunks cannot contain null.", nameof(chunks)));
        readable.push(null);
        return readable;
    }

    /// <summary>
    /// Is true if it is safe to call read().
    /// </summary>
    public bool readable { get { lock (_readLock) return !_endEmitted && !_destroyed; } }

    /// <summary>
    /// Becomes true when 'end' event is emitted.
    /// </summary>
    public bool readableEnded { get { lock (_readLock) return _endEmitted; } }

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
        return ReadCore(size);
    }

    private object? ReadCore(int? size)
    {
        if (size is <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Read size must be positive.");
        var requestRead = false;
        lock (_readLock)
        {
            if (_destroyed) return null;
            _didRead = true;
            if (_buffer.Count == 0 && !_ended && !_reading)
            {
                _reading = true;
                requestRead = true;
            }
        }
        if (requestRead)
            _read(size ?? 64 * 1024);

        object? chunk;
        lock (_readLock)
        {
            if (_buffer.Count == 0)
            {
                chunk = null;
            }
            else
            {
                ref var first = ref _buffer.First;
                if (size is int requested && first.Value is Buffer buffer && requested < buffer.length)
                {
                    chunk = buffer.subarray(0, requested);
                    first = new ReadableChunk(buffer.subarray(requested), first.Size - requested);
                    _bufferedSize = checked(_bufferedSize - requested);
                }
                else
                {
                    chunk = first.Value;
                    _bufferedSize = checked(_bufferedSize - first.Size);
                    _buffer.RemoveFirst();
                }
                SignalReadCapacityIfAvailable();
            }
        }
        ScheduleEndIfReady();
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

        RequestFlow();

        return this;
    }

    private void Flow()
    {
        lock (_readLock)
        {
            _flowRequested = true;
            if (_flowActive)
                return;
            _flowActive = true;
        }
        try
        {
            while (true)
            {
                lock (_readLock)
                    _flowRequested = false;
                while (true)
                {
                    lock (_readLock)
                    {
                        if (_paused || _destroyed)
                            break;
                    }
                    var chunk = ReadCore(null);
                    if (chunk == null)
                        break;
                    emit("data", chunk);
                }
                ScheduleEndIfReady();
                lock (_readLock)
                {
                    if (_flowRequested)
                        continue;
                    _flowActive = false;
                    return;
                }
            }
        }
        catch
        {
            lock (_readLock)
                _flowActive = false;
            throw;
        }
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
        where TDestination : Stream, IWritableStream
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
            var size = ChunkSize(chunk);
            var bufferedSize = checked(_bufferedSize + size);
            _buffer.AddFirst(new ReadableChunk(chunk, size));
            _bufferedSize = bufferedSize;
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
        var flow = false;
        var accepted = false;
        lock (_readLock)
        {
            if (_destroyed)
                return false;
            if (chunk == null)
            {
                _reading = false;
                _ended = true;
                flow = _flowing;
            }
            else
            {
                _reading = false;
                var normalizedChunk = _encoding == null
                    ? chunk
                    : chunk switch
                    {
                        Buffer buffer => buffer.toString(_encoding),
                        byte[] bytes => Buffer.Decode(bytes, _encoding),
                        _ => chunk,
                    };
                var size = ChunkSize(normalizedChunk);
                var bufferedSize = checked(_bufferedSize + size);
                _buffer.AddLast(new ReadableChunk(normalizedChunk, size));
                _bufferedSize = bufferedSize;
                flow = _flowing;
                if (!flow)
                {
                    BlockReadCapacityIfNeeded();
                    emitReadable = true;
                }
                accepted = _bufferedSize < _highWaterMark;
            }
        }

        if (flow)
        {
            RequestFlow();
            lock (_readLock)
                accepted = _bufferedSize < _highWaterMark;
        }
        if (emitReadable)
            emit("readable");
        ScheduleEndIfReady();

        return chunk != null && accepted;
    }

    /// <summary>
    /// Destroys the stream.
    /// </summary>
    /// <param name="error">Optional error to emit.</param>
    public override void destroy(Exception? error = null)
    {
        DestroyInput();
        base.destroy(error);
    }

    internal void DestroyInput()
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
        if (eventName is not string text || !string.Equals(text, "data", StringComparison.Ordinal))
            return;
        lock (_readLock)
        {
            if (_destroyed || _flowing)
                return;
            _paused = false;
            _flowing = true;
        }
        RequestFlow();
    }

    private void RequestFlow()
    {
        Action dispatch;
        lock (_readLock)
        {
            if (_destroyed || _paused) return;
            _flowRequested = true;
            if (_flowActive || _flowScheduled) return;
            _flowScheduled = true;
            dispatch = _flowDispatch ??= FlowFromListener;
        }
        try
        {
            Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(dispatch);
        }
        catch
        {
            lock (_readLock)
            {
                _flowScheduled = false;
            }
            throw;
        }
    }

    private void FlowFromListener()
    {
        lock (_readLock)
        {
            _flowScheduled = false;
            if (_paused || _destroyed)
                return;
        }
        Flow();
    }

    /// <summary>Destroys the stream and returns it for the source-level fluent contract.</summary>
    public Readable destroyChain(Exception? error = null)
    {
        destroy(error);
        return this;
    }

    private void ScheduleEndIfReady()
    {
        lock (_readLock)
        {
            if (!_didRead || _destroyed || !_ended || _buffer.Count != 0 || _endEmitted || _endScheduled)
                return;
            _endScheduled = true;
        }
        try
        {
            Tsonic.CSharp.Js.JsEventLoop.EnqueueHandleOwned(DeliverEnd);
        }
        catch
        {
            lock (_readLock) _endScheduled = false;
            throw;
        }
    }

    private void DeliverEnd()
    {
        lock (_readLock)
        {
            _endScheduled = false;
            if (_destroyed || !_ended || _buffer.Count != 0 || _endEmitted) return;
            _endEmitted = true;
        }
        emit("end");
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
