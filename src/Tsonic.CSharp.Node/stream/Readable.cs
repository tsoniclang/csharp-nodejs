using System;
using System.Collections.Generic;
using System.Text;

namespace Tsonic.CSharp.Node;

/// <summary>
/// A readable stream is an abstraction for a source from which data is read.
/// </summary>
public partial class Readable : Stream
{
    private readonly Queue<object?> _buffer = new Queue<object?>();
    private bool _ended = false;
    private bool _flowing = false;
    private string? _encoding;
    private bool _paused = true;

    /// <summary>
    /// Is true if it is safe to call read().
    /// </summary>
    public bool readable => !_ended && !destroyed;

    /// <summary>
    /// Becomes true when 'end' event is emitted.
    /// </summary>
    public bool readableEnded => _ended;

    /// <summary>
    /// This property reflects the current state of a Readable stream.
    /// </summary>
    public bool? readableFlowing => _flowing ? true : (_paused ? false : null);

    /// <summary>
    /// This property contains the number of bytes (or objects) in the queue ready to be read.
    /// </summary>
    public int readableLength => _buffer.Count;

    /// <summary>
    /// Is true after destroy() has been called.
    /// </summary>
    public bool destroyed { get; private set; }

    /// <summary>
    /// Reads data out of the internal buffer and returns it.
    /// </summary>
    /// <param name="size">Optional argument to specify how much data to read.</param>
    /// <returns>The data read, or null if no data is available.</returns>
    public object? read(int? size = null)
    {
        if (_buffer.Count == 0)
        {
            if (_ended)
            {
                emit("end");
            }
            return null;
        }

        var chunk = _buffer.Dequeue();

        if (_buffer.Count == 0 && _ended)
        {
            emit("end");
        }

        return chunk;
    }

    /// <summary>
    /// Sets the character encoding for data read from the Readable stream.
    /// </summary>
    /// <param name="encoding">The encoding to use.</param>
    /// <returns>This stream.</returns>
    public Readable setEncoding(string encoding)
    {
        _encoding = encoding;
        return this;
    }

    /// <summary>
    /// Causes a stream in flowing mode to stop emitting 'data' events, switching out of flowing mode.
    /// </summary>
    /// <returns>This stream.</returns>
    public Readable pause()
    {
        _paused = true;
        _flowing = false;
        return this;
    }

    /// <summary>
    /// Causes an explicitly paused Readable stream to resume emitting 'data' events, switching the stream into flowing mode.
    /// </summary>
    /// <returns>This stream.</returns>
    public Readable resume()
    {
        _paused = false;
        _flowing = true;

        // Emit buffered data
        while (_buffer.Count > 0)
        {
            var chunk = _buffer.Dequeue();
            emit("data", chunk);
        }

        if (_ended)
        {
            emit("end");
        }

        return this;
    }

    /// <summary>
    /// Returns the current operating state of the Readable.
    /// </summary>
    /// <returns>True if the stream is paused.</returns>
    public bool isPaused()
    {
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

    /// <summary>
    /// Pushes a chunk of data back into the internal buffer.
    /// </summary>
    /// <param name="chunk">Chunk of data to unshift onto the read queue.</param>
    public void unshift(object? chunk)
    {
        if (chunk != null)
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
        if (chunk == null)
        {
            // Pushing null signals end of stream
            _ended = true;
            if (_flowing)
            {
                emit("end");
            }
            return false;
        }

        _buffer.Enqueue(chunk);

        if (_flowing)
        {
            // In flowing mode, emit data immediately
            while (_buffer.Count > 0)
            {
                var data = _buffer.Dequeue();
                emit("data", data);
            }

            if (_ended)
            {
                emit("end");
            }
        }
        else
        {
            // In paused mode, emit 'readable' event
            emit("readable");
        }

        return true;
    }

    /// <summary>
    /// Destroys the stream.
    /// </summary>
    /// <param name="error">Optional error to emit.</param>
    public override void destroy(Exception? error = null)
    {
        if (destroyed)
            return;

        destroyed = true;
        _buffer.Clear();

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
}
