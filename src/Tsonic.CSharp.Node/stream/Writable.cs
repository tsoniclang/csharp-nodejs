using System;
using System.Collections.Generic;

namespace Tsonic.CSharp.Node;

/// <summary>
/// A writable stream is an abstraction for a destination to which data is written.
/// </summary>
public partial class Writable : Stream
{
    private readonly Queue<WriteRequest> _buffer = new Queue<WriteRequest>();
    private bool _ended = false;
    private bool _writing = false;
    private bool _corked = false;

    private class WriteRequest
    {
        public object? Chunk { get; set; }
        public string? Encoding { get; set; }
        public Action? Callback { get; set; }
    }

    /// <summary>
    /// Is true if it is safe to call write().
    /// </summary>
    public bool writable => !_ended && !destroyed;

    /// <summary>
    /// Is true after writable.end() has been called.
    /// </summary>
    public bool writableEnded => _ended;

    /// <summary>
    /// Is true after destroy() has been called.
    /// </summary>
    public bool destroyed { get; private set; }

    /// <summary>
    /// Number of bytes (or objects) in the write queue ready to be written.
    /// </summary>
    public int writableLength => _buffer.Count;

    /// <summary>
    /// Is true if the stream's buffer has been corked.
    /// </summary>
    public bool writableCorked => _corked;

    /// <summary>
    /// Writes data to the stream.
    /// </summary>
    /// <param name="chunk">The data to write.</param>
    /// <param name="encoding">The encoding if chunk is a string.</param>
    /// <param name="callback">Callback for when this chunk of data is flushed.</param>
    /// <returns>False if the stream wishes for the calling code to wait for the 'drain' event to be emitted before continuing to write.</returns>
    public bool write(object? chunk, string? encoding = null, Action? callback = null)
    {
        if (_ended)
        {
            throw new InvalidOperationException("write after end");
        }

        var request = new WriteRequest
        {
            Chunk = chunk,
            Encoding = encoding,
            Callback = callback
        };

        _buffer.Enqueue(request);

        if (!_corked)
        {
            ProcessWrites();
        }

        // Simplified: always return true (no backpressure handling in basic implementation)
        return true;
    }

    /// <summary>
    /// Signals that no more data will be written to the Writable.
    /// </summary>
    /// <param name="chunk">Optional data to write before ending.</param>
    /// <param name="encoding">The encoding if chunk is a string.</param>
    /// <param name="callback">Optional callback for when the stream has finished.</param>
    public void end(object? chunk = null, string? encoding = null, Action? callback = null)
    {
        if (chunk != null)
        {
            write(chunk, encoding);
        }

        if (callback != null)
        {
            once("finish", callback);
        }

        _ended = true;

        if (!_corked)
        {
            ProcessWrites();
        }

        if (_buffer.Count == 0)
        {
            emit("finish");
        }
    }

    /// <summary>
    /// Forces all written data to be buffered in memory. The buffered data will be flushed when uncork() is called.
    /// </summary>
    public void cork()
    {
        _corked = true;
    }

    /// <summary>
    /// Flushes all data buffered since cork() was called.
    /// </summary>
    public void uncork()
    {
        _corked = false;
        ProcessWrites();
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

    private void ProcessWrites()
    {
        if (_writing || _buffer.Count == 0)
            return;

        _writing = true;

        while (_buffer.Count > 0)
        {
            var request = _buffer.Dequeue();
            _write(request.Chunk, request.Encoding, () =>
            {
                request.Callback?.Invoke();
            });
        }

        _writing = false;

        if (_ended && _buffer.Count == 0)
        {
            emit("finish");
        }
    }

    /// <summary>
    /// Internal method to be implemented by subclasses to write data.
    /// </summary>
    /// <param name="chunk">Chunk of data to write.</param>
    /// <param name="encoding">Encoding if chunk is a string.</param>
    /// <param name="callback">Callback for when write is complete.</param>
    protected virtual void _write(object? chunk, string? encoding, Action callback)
    {
        // To be implemented by subclasses
        callback();
    }

    /// <summary>
    /// Internal method called right before the stream closes.
    /// </summary>
    /// <param name="callback">Callback for when finalize is complete.</param>
    protected virtual void _final(Action callback)
    {
        // To be implemented by subclasses
        callback();
    }
}
