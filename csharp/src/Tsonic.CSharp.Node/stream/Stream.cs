using System;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Base class for all streams. A stream is an abstract interface for working with streaming data.
/// </summary>
public class Stream : EventEmitter
{
    private bool _destroyStarted;
    private bool _closeEmitted;
    private Exception? _destroyError;

    internal void BeginDestroy(Exception? error)
    {
        if (_destroyStarted)
            return;
        _destroyStarted = true;
        _destroyError = error;
    }

    /// <summary>
    /// Pipes the output of this readable stream into a writable stream destination.
    /// </summary>
    /// <param name="destination">The destination writable stream.</param>
    /// <param name="end">Whether to end the destination when this stream ends. Default is true.</param>
    /// <returns>The destination stream.</returns>
    public virtual Stream pipe(Stream destination, bool end = true)
    {
        if (this is not Readable readable)
        {
            throw new InvalidOperationException("pipe() can only be called on Readable streams");
        }

        if (destination is not IWritableStream writable)
        {
            throw new InvalidOperationException("pipe() destination must be a Writable stream");
        }

        var pumping = false;
        var pumpRequested = false;
        var completed = false;
        var endRequested = false;
        Action? pump = null;
        Action? onReadable = null;
        Action? onEnd = null;
        Action? onDrain = null;

        void Complete()
        {
            if (completed)
                return;
            if (pumping)
            {
                endRequested = true;
                return;
            }
            completed = true;
            if (onReadable is not null)
                readable.off("readable", onReadable);
            if (onEnd is not null)
                readable.off("end", onEnd);
            if (onDrain is not null)
                destination.off("drain", onDrain);
            if (!end)
                return;
            writable.EndChunk();
        }

        pump = () =>
        {
            if (completed)
                return;
            if (pumping)
            {
                pumpRequested = true;
                return;
            }

            pumping = true;
            try
            {
                do
                {
                    pumpRequested = false;
                    while (!completed)
                    {
                        var chunk = readable.read();
                        if (chunk is null)
                        {
                            if (readable.readableEnded)
                                Complete();
                            break;
                        }

                        var accepted = writable.WriteChunk(chunk);
                        if (!accepted)
                        {
                            readable.pause();
                            onDrain = () =>
                            {
                                onDrain = null;
                                pump!();
                            };
                            destination.once("drain", onDrain);
                            break;
                        }
                    }
                }
                while (pumpRequested && !completed);
            }
            finally
            {
                pumping = false;
                if (endRequested)
                    Complete();
            }
        };

        onReadable = () => pump();
        onEnd = Complete;
        readable.on("readable", onReadable);
        readable.once("end", onEnd);
        pump();

        return destination;
    }

    /// <summary>
    /// Destroys the stream and optionally emits an error event.
    /// </summary>
    /// <param name="error">Optional error to emit.</param>
    public virtual void destroy(Exception? error = null)
    {
        BeginDestroy(error);
        var pendingError = _destroyError;
        _destroyError = null;
        if (pendingError is not null)
            emit("error", pendingError);
        if (!_closeEmitted)
        {
            _closeEmitted = true;
            emit("close");
        }
    }
}
