#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

public sealed class ReadableStreamDefaultController
{
    private readonly ReadableStream _stream;

    public ReadableStreamDefaultController(ReadableStream stream)
    {
        _stream = stream;
    }

    public int? desiredSize => null;

    public void enqueue(object? chunk) => _stream.Enqueue(chunk);

    public void close() => _stream.Close();

    public void error(object? error) => throw new InvalidOperationException(error?.ToString());
}

public sealed class ReadableByteStreamController
{
    public ReadableByteStreamController(ReadableStream stream)
    {
        _stream = stream;
    }

    private readonly ReadableStream _stream;

    public int? desiredSize => null;

    public ReadableStreamBYOBRequest? byobRequest { get; private set; }

    public void setByobRequest(ReadableStreamBYOBRequest request) => byobRequest = request;

    public void enqueue(byte[] chunk) => _stream.Enqueue(chunk);

    public void close() => _stream.Close();

    public void error(object? error) => throw new InvalidOperationException(error?.ToString());
}

public sealed class ReadableStreamBYOBRequest
{
    public ReadableStreamBYOBRequest(byte[] view)
    {
        this.view = view;
    }

    public byte[] view { get; private set; }

    public int bytesWritten { get; private set; }

    public void respond(int bytesWritten)
    {
        this.bytesWritten = bytesWritten;
    }

    public void respondWithNewView(byte[] view)
    {
        this.view = view;
        bytesWritten = view.Length;
    }
}

public sealed class WritableStreamDefaultController
{
    public CancellationToken signal { get; set; }

    public bool abortSignal => signal.IsCancellationRequested;

    public bool errored { get; private set; }

    public void error(object? error)
    {
        _ = error;
        errored = true;
    }
}

public sealed class TransformStreamDefaultController
{
    private readonly List<object?> _chunks = [];

    public int? desiredSize => null;

    public object?[] chunks => _chunks.ToArray();

    public void enqueue(object? chunk) => _chunks.Add(chunk);

    public void error(object? error) => throw new InvalidOperationException(error?.ToString());

    public void terminate() => _chunks.Clear();
}
