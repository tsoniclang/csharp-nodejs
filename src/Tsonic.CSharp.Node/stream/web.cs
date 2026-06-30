#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public sealed class StreamPipeOptions
{
    public bool preventClose { get; set; }

    public bool preventAbort { get; set; }

    public bool preventCancel { get; set; }

    public CancellationToken signal { get; set; }
}

public sealed class ReadableStreamGetReaderOptions
{
    public string? mode { get; set; }
}

public sealed class ReadableStreamIteratorOptions
{
    public bool preventCancel { get; set; }
}

public sealed class ReadableStreamBYOBReaderReadOptions
{
    public int? min { get; set; }
}

public sealed class ReadableStreamReadResult
{
    public object? value { get; set; }

    public bool done { get; set; }
}

public sealed class ReadableStream
{
    private readonly Queue<object?> _chunks;
    private bool _locked;
    private bool _canceled;

    public ReadableStream()
    {
        _chunks = [];
    }

    private ReadableStream(IEnumerable<object?> chunks)
    {
        _chunks = new Queue<object?>(chunks);
    }

    public static ReadableStream fromChunks(params object?[] chunks)
    {
        return new ReadableStream(chunks);
    }

    public object?[] chunks => _chunks.ToArray();

    public bool locked => _locked;

    public bool canceled => _canceled;

    public ReadableStreamDefaultReader getReader(ReadableStreamGetReaderOptions? options = null)
    {
        _ = options;
        if (_locked)
            throw new InvalidOperationException("ReadableStream is already locked.");

        _locked = true;
        return new ReadableStreamDefaultReader(this);
    }

    public Task cancel(object? reason = null)
    {
        _ = reason;
        _canceled = true;
        _chunks.Clear();
        return Task.CompletedTask;
    }

    public IEnumerable<object?> values(ReadableStreamIteratorOptions? options = null)
    {
        _ = options;
        while (_chunks.Count > 0)
            yield return _chunks.Dequeue();
    }

    public async Task pipeTo(WritableStream destination, StreamPipeOptions? options = null)
    {
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        options?.signal.ThrowIfCancellationRequested();
        foreach (var chunk in values())
            await destination.write(chunk).ConfigureAwait(false);

        if (options?.preventClose != true)
            await destination.close().ConfigureAwait(false);
    }

    public ReadableStream[] tee()
    {
        var snapshot = _chunks.ToArray();
        return [new ReadableStream(snapshot), new ReadableStream(snapshot)];
    }

    internal ReadableStreamReadResult ReadOne()
    {
        if (_chunks.Count == 0)
            return new ReadableStreamReadResult { done = true };

        return new ReadableStreamReadResult { value = _chunks.Dequeue(), done = false };
    }

    internal void ReleaseLock()
    {
        _locked = false;
    }

    internal void Enqueue(object? chunk)
    {
        _chunks.Enqueue(chunk);
    }

    internal void Close()
    {
    }
}
