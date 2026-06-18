using System;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Base class for all streams. A stream is an abstract interface for working with streaming data.
/// </summary>
public class Stream : EventEmitter
{
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

        // Check if destination can be written to (Writable or Duplex)
        bool canWrite = destination is Writable || destination is Duplex;
        if (!canWrite)
        {
            throw new InvalidOperationException("pipe() destination must be a Writable stream");
        }

        // Set up data forwarding
        readable.on("data", (Action<object?>)(chunk =>
        {
            if (destination is Duplex duplex)
            {
                duplex.write(chunk);
            }
            else if (destination is Writable writable)
            {
                writable.write(chunk);
            }
        }));

        // Handle end event
        if (end)
        {
            readable.on("end", (Action)(() =>
            {
                if (destination is Duplex duplex)
                {
                    duplex.end();
                }
                else if (destination is Writable writable)
                {
                    writable.end();
                }
            }));
        }

        // Handle errors
        readable.on("error", (Action<Exception>)(err =>
        {
            destination.emit("error", err);
        }));

        // Start flowing
        readable.resume();

        return destination;
    }

    /// <summary>
    /// Destroys the stream and optionally emits an error event.
    /// </summary>
    /// <param name="error">Optional error to emit.</param>
    public virtual void destroy(Exception? error = null)
    {
        if (error != null)
        {
            emit("error", error);
        }

        emit("close");
    }
}
