using System;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public static partial class stream
{
    private static readonly StreamPromises _promises = new();

    /// <summary>
    /// Promise-based helpers for stream utilities.
    /// </summary>
    public static StreamPromises promises => _promises;
}

/// <summary>
/// Promise-based stream helpers.
/// </summary>
public class StreamPromises
{
    /// <summary>
    /// Promise-based pipeline.
    /// </summary>
    public Task pipeline(params Stream[] streams)
    {
        if (streams == null || streams.Length < 2)
            throw new ArgumentException("pipeline requires at least two streams", nameof(streams));

        var tcs = new TaskCompletionSource();
        var args = new object[streams.Length + 1];
        for (var i = 0; i < streams.Length; i++)
        {
            args[i] = streams[i];
        }

        args[^1] = (Action<Exception?>)(error =>
        {
            if (error != null) tcs.TrySetException(error);
            else tcs.TrySetResult();
        });

        stream.pipeline(args);
        return tcs.Task;
    }

    /// <summary>
    /// Promise-based finished.
    /// </summary>
    public Task finished(Stream streamValue)
    {
        return stream.finished(streamValue);
    }
}
