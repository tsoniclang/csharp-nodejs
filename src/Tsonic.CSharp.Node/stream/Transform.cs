using System;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Transform streams are Duplex streams where the output is computed from the input.
/// </summary>
public class Transform : Duplex
{
    /// <summary>
    /// Internal method to be implemented by subclasses to transform data.
    /// </summary>
    /// <param name="chunk">Chunk of data to transform.</param>
    /// <param name="encoding">Encoding if chunk is a string.</param>
    /// <param name="callback">Callback for when transform is complete. Call with (error, data).</param>
    protected virtual void _transform(object? chunk, string? encoding, Action<Exception?, object?> callback)
    {
        // Default implementation: pass through
        callback(null, chunk);
    }

    /// <summary>
    /// Internal method called right before the stream closes, to process any remaining data.
    /// </summary>
    /// <param name="callback">Callback for when flush is complete.</param>
    protected virtual void _flush(Action<Exception?> callback)
    {
        // To be implemented by subclasses
        callback(null);
    }

    /// <summary>
    /// Internal method to write data to the transform stream.
    /// </summary>
    /// <param name="chunk">Chunk of data to write.</param>
    /// <param name="encoding">Encoding if chunk is a string.</param>
    /// <param name="callback">Callback for when write is complete.</param>
    protected override void _write(object? chunk, string? encoding, Action callback)
    {
        _transform(chunk, encoding, (error, data) =>
        {
            if (error != null)
            {
                emit("error", error);
                callback();
                return;
            }

            if (data != null)
            {
                push(data);
            }

            callback();
        });
    }
}
