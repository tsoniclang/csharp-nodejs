using System;

namespace Tsonic.CSharp.Node;

/// <summary>
/// PassThrough streams are a trivial implementation of a Transform stream that simply passes the input bytes across to the output.
/// </summary>
public class PassThrough : Transform
{
    /// <summary>
    /// Transforms data by passing it through unchanged.
    /// </summary>
    /// <param name="chunk">Chunk of data to transform.</param>
    /// <param name="encoding">Encoding if chunk is a string.</param>
    /// <param name="callback">Callback for when transform is complete.</param>
    protected override void _transform(object? chunk, string? encoding, Action<Exception?, object?> callback)
    {
        // Just pass the data through unchanged
        callback(null, chunk);
    }
}
