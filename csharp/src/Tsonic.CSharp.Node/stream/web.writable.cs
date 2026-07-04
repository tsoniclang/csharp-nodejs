#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public sealed class ReadableStreamDefaultReader
{
    private readonly ReadableStream _stream;
    private bool _released;

    internal ReadableStreamDefaultReader(ReadableStream stream)
    {
        _stream = stream;
    }

    public Task closed => Task.CompletedTask;

    public Task<ReadableStreamReadResult> read()
    {
        if (_released)
            throw new InvalidOperationException("Reader lock has been released.");

        return Task.FromResult(_stream.ReadOne());
    }

    public ReadableStreamReadResult readResult()
    {
        return read().GetAwaiter().GetResult();
    }

    public Task cancel(object? reason = null)
    {
        return _stream.cancel(reason);
    }

    public void releaseLock()
    {
        _released = true;
        _stream.ReleaseLock();
    }
}

public sealed class WritableStream
{
    private readonly List<object?> _chunks = [];
    private bool _locked;
    private bool _closed;
    private bool _aborted;

    public object?[] chunks => _chunks.ToArray();

    public bool locked => _locked;

    public bool closed => _closed;

    public bool aborted => _aborted;

    public Task write(object? chunk)
    {
        if (_closed)
            throw new InvalidOperationException("WritableStream is closed.");

        _chunks.Add(chunk);
        return Task.CompletedTask;
    }

    public Task close()
    {
        _closed = true;
        return Task.CompletedTask;
    }

    public Task abort(object? reason = null)
    {
        _ = reason;
        _aborted = true;
        _closed = true;
        return Task.CompletedTask;
    }

    public WritableStreamDefaultWriter getWriter()
    {
        if (_locked)
            throw new InvalidOperationException("WritableStream is already locked.");

        _locked = true;
        return new WritableStreamDefaultWriter(this);
    }

    internal void ReleaseLock()
    {
        _locked = false;
    }
}

public sealed class WritableStreamDefaultWriter
{
    private readonly WritableStream _stream;

    internal WritableStreamDefaultWriter(WritableStream stream)
    {
        _stream = stream;
    }

    public Task ready => Task.CompletedTask;

    public Task closed => _stream.closed ? Task.CompletedTask : Task.CompletedTask;

    public int? desiredSize => null;

    public Task write(object? chunk) => _stream.write(chunk);

    public Task close() => _stream.close();

    public Task abort(object? reason = null) => _stream.abort(reason);

    public void releaseLock() => _stream.ReleaseLock();
}
