using System;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Duplex streams are streams that implement both the Readable and Writable interfaces.
/// </summary>
public partial class Duplex : Readable
{
    private readonly WritableState _writableState;

    /// <summary>
    /// Creates a new Duplex stream.
    /// </summary>
    public Duplex()
    {
        _writableState = new WritableState(
            (chunk, encoding, callback) => _write(chunk, encoding, callback),
            callback => _final(callback),
            eventName => emit(eventName));
        _writableState.SetFinishCallbackRegistrar(callback => once("finish", callback));
    }

    // Writable interface

    /// <summary>
    /// Is true if it is safe to call write().
    /// </summary>
    public bool writable => _writableState.Writable;

    /// <summary>
    /// Is true after writable.end() has been called.
    /// </summary>
    public bool writableEnded => _writableState.Ended;

    /// <summary>Is true after the native writable side has finalized.</summary>
    public bool writableFinished => _writableState.Finished;

    /// <summary>Is true while producers must wait for the drain event.</summary>
    public bool writableNeedDrain => _writableState.NeedDrain;

    /// <summary>
    /// Number of bytes (or objects) in the write queue ready to be written.
    /// </summary>
    public long writableLength => _writableState.BufferedSize;

    /// <summary>
    /// Is true if the stream's buffer has been corked.
    /// </summary>
    public bool writableCorked => _writableState.Corked;

    /// <summary>
    /// Writes data to the stream.
    /// </summary>
    /// <param name="chunk">The data to write.</param>
    /// <returns>False if the stream wishes for the calling code to wait for the 'drain' event to be emitted before continuing to write.</returns>
    public bool write(string chunk) => WriteChunk(chunk);

    /// <summary>Writes one binary chunk to the writable side.</summary>
    public bool write(Buffer chunk) => WriteChunk(chunk);

    /// <summary>
    /// Signals that no more data will be written to the Writable.
    /// </summary>
    public Duplex end()
    {
        _writableState.End(null, null, null);
        return this;
    }

    /// <summary>Finishes the writable side after one final text chunk.</summary>
    public Duplex end(string chunk)
    {
        _writableState.End(chunk, null, null);
        return this;
    }

    /// <summary>Finishes the writable side after one final binary chunk.</summary>
    public Duplex end(Buffer chunk)
    {
        _writableState.End(chunk, null, null);
        return this;
    }

    /// <summary>Writes a native stream chunk to the writable side.</summary>
    protected internal bool WriteChunk(
        object? chunk,
        string? encoding = null,
        Action? callback = null) => _writableState.Write(chunk, encoding, callback);

    /// <summary>Finishes the writable side after an optional native chunk.</summary>
    protected internal void EndChunk(
        object? chunk = null,
        string? encoding = null,
        Action? callback = null) => _writableState.End(chunk, encoding, callback);

    /// <summary>
    /// Forces all written data to be buffered in memory. The buffered data will be flushed when uncork() is called.
    /// </summary>
    public void cork()
    {
        _writableState.Cork();
    }

    /// <summary>
    /// Flushes all data buffered since cork() was called.
    /// </summary>
    public void uncork()
    {
        _writableState.Uncork();
    }

    /// <summary>
    /// Destroys the stream.
    /// </summary>
    /// <param name="error">Optional error to emit.</param>
    public override void destroy(Exception? error = null)
    {
        _writableState.Destroy();
        base.destroy(error);
    }

    /// <summary>Destroys both sides and returns this stream.</summary>
    public new Duplex destroyChain(Exception? error = null)
    {
        destroy(error);
        return this;
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

    /// <summary>Finalizes the writable side of the duplex stream.</summary>
    protected virtual void _final(Action callback)
    {
        callback();
    }
}
