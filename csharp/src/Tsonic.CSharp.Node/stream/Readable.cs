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
    private readonly Queue<object?> _buffer = new Queue<object?>();
    private readonly object _readLock = new();
    private readonly ManualResetEventSlim _capacityAvailable = new(true);
    private readonly int _highWaterMark;
    private long _bufferedSize;
    private bool _ended = false;
    private bool _flowing = false;
    private string? _encoding;
    private bool _paused = true;
    private bool _reading;
    private bool _endEmitted;

    public Readable()
        : this(64 * 1024)
    {
    }

    protected Readable(int highWaterMark)
    {
        if (highWaterMark <= 0)
            throw new ArgumentOutOfRangeException(nameof(highWaterMark));
        _highWaterMark = highWaterMark;
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
    public bool destroyed { get { lock (_readLock) return _destroyed; } }

    /// <summary>
    /// Reads data out of the internal buffer and returns it.
    /// </summary>
    /// <param name="size">Optional argument to specify how much data to read.</param>
    /// <returns>The data read, or null if no data is available.</returns>
    public object? read(int? size = null)
    {
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
                chunk = _buffer.Dequeue();
                _bufferedSize = checked(_bufferedSize - ChunkSize(chunk));
                if (_bufferedSize < _highWaterMark)
                    _capacityAvailable.Set();
                emitEnd = MarkEndReady();
            }
        }
        if (emitEnd)
            emit("end");
        return chunk;
    }

    public TsValue readValue(int? size = null)
    {
        var value = read(size);
        return value is null ? TsValue.undefined() : TsValue.from(value);
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

    public Writable pipeTo(Writable destination)
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
            // Create a new queue with the chunk at the front
            var newBuffer = new Queue<object?>();
            newBuffer.Enqueue(chunk);
            while (_buffer.Count > 0)
            {
                newBuffer.Enqueue(_buffer.Dequeue());
            }

            // Replace buffer
            while (newBuffer.Count > 0)
            {
                _buffer.Enqueue(newBuffer.Dequeue());
            }
            _bufferedSize = checked(_bufferedSize + ChunkSize(chunk));
            if (_bufferedSize >= _highWaterMark)
                _capacityAvailable.Reset();
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
                _buffer.Enqueue(normalizedChunk);
                _bufferedSize = checked(_bufferedSize + ChunkSize(normalizedChunk));
                if (_flowing)
                {
                    flowingChunks = new List<object?>(_buffer.Count);
                    while (_buffer.Count > 0)
                    {
                        var buffered = _buffer.Dequeue();
                        flowingChunks.Add(buffered);
                        _bufferedSize = checked(_bufferedSize - ChunkSize(buffered));
                    }
                    _capacityAvailable.Set();
                    emitEnd = MarkEndReady();
                }
                else
                {
                    if (_bufferedSize >= _highWaterMark)
                        _capacityAvailable.Reset();
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
            _capacityAvailable.Set();
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

    protected void WaitForReadCapacity(CancellationToken cancellationToken = default)
    {
        _capacityAvailable.Wait(cancellationToken);
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
