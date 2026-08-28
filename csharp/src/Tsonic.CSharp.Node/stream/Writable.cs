using System;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

/// <summary>
/// A writable stream is an abstraction for a destination to which data is written.
/// </summary>
public partial class Writable : Stream
{
    private readonly WritableState _state;

    public Writable()
        : this(64 * 1024)
    {
    }

    protected Writable(int highWaterMark)
    {
        _state = new WritableState(
            (chunk, encoding, callback) => _write(chunk, encoding, callback),
            callback => _final(callback),
            eventName => emit(eventName),
            highWaterMark);
        _state.SetFinishCallbackRegistrar(callback => once("finish", callback));
    }

    /// <summary>
    /// Is true if it is safe to call write().
    /// </summary>
    public bool writable => _state.Writable;

    /// <summary>
    /// Is true after writable.end() has been called.
    /// </summary>
    public bool writableEnded => _state.Ended;

    /// <summary>
    /// Is true after destroy() has been called.
    /// </summary>
    public bool destroyed => _state.Destroyed;

    /// <summary>
    /// Number of bytes (or objects) in the write queue ready to be written.
    /// </summary>
    public long writableLength => _state.BufferedSize;

    /// <summary>
    /// Is true if the stream's buffer has been corked.
    /// </summary>
    public bool writableCorked => _state.Corked;

    /// <summary>
    /// Writes data to the stream.
    /// </summary>
    /// <param name="chunk">The data to write.</param>
    /// <param name="encoding">The encoding if chunk is a string.</param>
    /// <param name="callback">Callback for when this chunk of data is flushed.</param>
    /// <returns>False if the stream wishes for the calling code to wait for the 'drain' event to be emitted before continuing to write.</returns>
    public bool write(object? chunk, string? encoding = null, Action? callback = null)
    {
        return _state.Write(chunk, encoding, callback);
    }

    public bool write(TsValue chunk, string? encoding = null, Action? callback = null) =>
        write(chunk.unwrap(), encoding, callback);

    /// <summary>
    /// Signals that no more data will be written to the Writable.
    /// </summary>
    /// <param name="chunk">Optional data to write before ending.</param>
    /// <param name="encoding">The encoding if chunk is a string.</param>
    /// <param name="callback">Optional callback for when the stream has finished.</param>
    public void end(object? chunk = null, string? encoding = null, Action? callback = null)
    {
        _state.End(chunk, encoding, callback);
    }

    public void end(TsValue chunk, string? encoding = null, Action? callback = null) =>
        end(chunk.unwrap(), encoding, callback);

    /// <summary>
    /// Forces all written data to be buffered in memory. The buffered data will be flushed when uncork() is called.
    /// </summary>
    public void cork()
    {
        _state.Cork();
    }

    /// <summary>
    /// Flushes all data buffered since cork() was called.
    /// </summary>
    public void uncork()
    {
        _state.Uncork();
    }

    /// <summary>
    /// Destroys the stream.
    /// </summary>
    /// <param name="error">Optional error to emit.</param>
    public override void destroy(Exception? error = null)
    {
        if (_state.Destroyed)
            return;
        _state.Destroy();
        base.destroy(error);
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
