using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// The stream module provides utility functions for working with streams.
/// </summary>
public static partial class stream
{
    /// <summary>
    /// A method to pipe between streams forwarding errors and properly cleaning up.
    /// </summary>
    /// <param name="streams">The streams to pipe together, followed by an optional callback.</param>
    public static void pipeline(params object[] streams)
    {
        if (streams == null || streams.Length < 2)
            throw new ArgumentException("pipeline requires at least a source and destination", nameof(streams));

        // Check if last argument is a callback
        Action<Exception?>? callback = null;
        var streamList = new List<Stream>();

        for (int i = 0; i < streams.Length; i++)
        {
            if (i == streams.Length - 1 && streams[i] is Action<Exception?> cb)
            {
                callback = cb;
            }
            else if (streams[i] is Stream s)
            {
                streamList.Add(s);
            }
            else
            {
                throw new ArgumentException($"Argument {i} is not a Stream or callback");
            }
        }

        if (streamList.Count < 2)
            throw new ArgumentException("pipeline requires at least a source and destination stream");

        try
        {
            // Pipe streams together
            for (int i = 0; i < streamList.Count - 1; i++)
            {
                var source = streamList[i];
                var dest = streamList[i + 1];

                // Set up error handling
                source.on("error", (Action<Exception>)(err =>
                {
                    // Destroy remaining streams
                    for (int j = i; j < streamList.Count; j++)
                    {
                        streamList[j].destroy(err);
                    }
                    callback?.Invoke(err);
                }));

                // Pipe source to destination
                source.pipe(dest, end: i == streamList.Count - 2);
            }

            // Handle final stream completion
            var lastStream = streamList[streamList.Count - 1];
            lastStream.on("finish", (Action)(() =>
            {
                callback?.Invoke(null);
            }));
            lastStream.on("end", (Action)(() =>
            {
                callback?.Invoke(null);
            }));
            lastStream.on("error", (Action<Exception>)(err =>
            {
                callback?.Invoke(err);
            }));
        }
        catch (Exception ex)
        {
            // Clean up all streams on error
            foreach (var s in streamList)
            {
                try { s.destroy(ex); } catch { }
            }
            callback?.Invoke(ex);
        }
    }

    /// <summary>
    /// A function to get notified when a stream is no longer readable, writable or has experienced an error or a premature close event.
    /// </summary>
    /// <param name="stream">The stream to monitor.</param>
    /// <param name="callback">The callback to invoke when the stream is finished.</param>
    public static void finished(Stream stream, Action<Exception?> callback)
    {
        if (stream == null)
            throw new ArgumentNullException(nameof(stream));
        if (callback == null)
            throw new ArgumentNullException(nameof(callback));

        bool called = false;

        // Declare action variables first
        Action onFinish = null!;
        Action onEnd = null!;
        Action<Exception> onError = null!;
        Action<bool> onClose = null!;

        void onFinished(Exception? error)
        {
            if (called) return;
            called = true;

            // Remove all listeners
            stream.removeListener("finish", onFinish);
            stream.removeListener("end", onEnd);
            stream.removeListener("error", onError);
            stream.removeListener("close", onClose);

            callback(error);
        }

        onFinish = () => onFinished(null);
        onEnd = () => onFinished(null);
        onError = (err) => onFinished(err);
        onClose = (hadError) => onFinished(hadError ? new Exception("Stream closed with error") : null);

        stream.on("finish", onFinish);
        stream.on("end", onEnd);
        stream.on("error", onError);
        stream.on("close", onClose);
    }

    /// <summary>
    /// A function to get notified when a stream is no longer readable, writable or has experienced an error or a premature close event.
    /// Returns a Promise that fulfills when the stream is finished.
    /// </summary>
    /// <param name="stream">The stream to monitor.</param>
    /// <returns>A Task that completes when the stream is finished.</returns>
    public static Task finished(Stream stream)
    {
        var tcs = new TaskCompletionSource();

        finished(stream, (error) =>
        {
            if (error != null)
                tcs.TrySetException(error);
            else
                tcs.TrySetResult();
        });

        return tcs.Task;
    }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
