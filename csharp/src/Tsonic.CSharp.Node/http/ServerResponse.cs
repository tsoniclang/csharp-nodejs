using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Tsonic.CSharp.Js;
using Tsonic.CSharp.Node;

namespace Tsonic.CSharp.Node.Http;

/// <summary>An ordered, pressure-aware HTTP response over Kestrel.</summary>
public partial class ServerResponse : Writable
{
    private readonly HttpResponse _response;
    private readonly TaskCompletionSource _completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly object _headersStartLock = new();
    private readonly CancellationTokenRegistration _abortRegistration;
    private Task? _headersStartTask;
    private int _headersStarted;
    private int _terminal;
    private Timer? _timeoutTimer;

    internal ServerResponse(HttpResponse response)
    {
        _response = response;
        _abortRegistration = response.HttpContext.RequestAborted.Register(AbortFromTransport);
    }

    /// <summary>HTTP status code sent to the client.</summary>
    public int statusCode
    {
        get => _response.StatusCode;
        set
        {
            EnsureHeadersMutable();
            if (value < 100 || value > 999)
                throw new ArgumentOutOfRangeException(nameof(value), "HTTP status code must be 100 through 999.");
            _response.StatusCode = value;
        }
    }

    /// <summary>HTTP/1 response reason phrase.</summary>
    public string statusMessage
    {
        get => _response.HttpContext.Features.Get<IHttpResponseFeature>()?.ReasonPhrase ?? string.Empty;
        set
        {
            EnsureHeadersMutable();
            var feature = _response.HttpContext.Features.Get<IHttpResponseFeature>()
                ?? throw new InvalidOperationException("The native HTTP response feature is unavailable.");
            feature.ReasonPhrase = value;
        }
    }

    /// <summary>True once native response headers have started.</summary>
    public bool headersSent => Volatile.Read(ref _headersStarted) != 0 || _response.HasStarted;

    /// <summary>Deprecated Node alias for writableEnded.</summary>
    public bool finished => writableEnded;

    internal Task Completion => _completion.Task;

    /// <summary>Sets a single response-header value.</summary>
    public ServerResponse setHeader(string name, string value)
    {
        SetHeaderValues(name, [value]);
        return this;
    }

    /// <summary>Sets exact repeated response-header values.</summary>
    public ServerResponse setHeader(string name, string[] values)
    {
        SetHeaderValues(name, values);
        return this;
    }

    /// <summary>Appends a single response-header value.</summary>
    public ServerResponse appendHeader(string name, string value)
    {
        AppendHeaderValues(name, [value]);
        return this;
    }

    /// <summary>Appends exact repeated response-header values.</summary>
    public ServerResponse appendHeader(string name, string[] values)
    {
        AppendHeaderValues(name, values);
        return this;
    }

    /// <summary>Returns the first stored header value, or native absence.</summary>
    public string? getHeader(string name)
    {
        http.validateHeaderName(name);
        return _response.Headers.TryGetValue(name, out var values) && values.Count > 0
            ? values[0]
            : null;
    }

    /// <summary>Returns every stored header value.</summary>
    public string[] getHeaderValues(string name)
    {
        http.validateHeaderName(name);
        return _response.Headers.TryGetValue(name, out var values)
            ? values.Select(value => value ?? throw new InvalidOperationException("Native HTTP header value is null.")).ToArray()
            : System.Array.Empty<string>();
    }

    /// <summary>Returns the unique stored response-header names.</summary>
    public string[] getHeaderNames() => _response.Headers.Keys.ToArray();

    /// <summary>Returns an immutable snapshot of all stored response headers.</summary>
    public OutgoingHttpHeaders getHeaders() =>
        new(_response.Headers.Select(header =>
            new KeyValuePair<string, IEnumerable<string>>(header.Key, header.Value)));

    /// <summary>Tests whether a response header is stored.</summary>
    public bool hasHeader(string name)
    {
        http.validateHeaderName(name);
        return _response.Headers.ContainsKey(name);
    }

    /// <summary>Removes a stored response header before native transmission starts.</summary>
    public void removeHeader(string name)
    {
        EnsureHeadersMutable();
        http.validateHeaderName(name);
        _response.Headers.Remove(name);
    }

    /// <summary>Writes a status line without changing stored headers.</summary>
    public ServerResponse writeHead(int statusCode)
    {
        this.statusCode = statusCode;
        MarkHeadersStarted();
        return this;
    }

    /// <summary>Writes a status line and exact stored headers.</summary>
    public ServerResponse writeHead(int statusCode, OutgoingHttpHeaders headers)
    {
        this.statusCode = statusCode;
        ApplyHeaders(headers);
        MarkHeadersStarted();
        return this;
    }

    /// <summary>Writes a status line, reason phrase and optional exact stored headers.</summary>
    public ServerResponse writeHead(
        int statusCode,
        string statusMessage,
        OutgoingHttpHeaders? headers = null)
    {
        this.statusCode = statusCode;
        this.statusMessage = statusMessage;
        if (headers is not null)
            ApplyHeaders(headers);
        MarkHeadersStarted();
        return this;
    }

    /// <summary>Queues a text body chunk.</summary>
    public new bool write(string chunk) => WriteChunk(chunk);

    /// <summary>Queues a binary body chunk.</summary>
    public new bool write(Buffer chunk) => WriteChunk(chunk);

    /// <summary>Ends the response without a final body chunk.</summary>
    public new ServerResponse end()
    {
        EndChunk();
        return this;
    }

    /// <summary>Ends the response with a final text chunk.</summary>
    public new ServerResponse end(string chunk)
    {
        EndChunk(chunk);
        return this;
    }

    /// <summary>Ends the response with a final binary chunk.</summary>
    public new ServerResponse end(Buffer chunk)
    {
        EndChunk(chunk);
        return this;
    }

    /// <summary>Aborts the response and returns it for fluent source code.</summary>
    public new ServerResponse destroyChain(Exception? error = null)
    {
        destroy(error);
        return this;
    }

    /// <inheritdoc />
    public override void destroy(Exception? error = null)
    {
        if (Interlocked.Exchange(ref _terminal, 1) != 0)
            return;
        _timeoutTimer?.Dispose();
        _abortRegistration.Dispose();
        _response.HttpContext.Abort();
        if (error is null)
            _completion.TrySetCanceled();
        else
            _completion.TrySetException(error);
        base.destroy(error);
    }

    /// <inheritdoc />
    protected override void _write(object? chunk, string? encoding, Action callback)
    {
        _ = WriteChunkAsync(chunk, encoding, callback);
    }

    /// <inheritdoc />
    protected override void _final(Action callback)
    {
        _ = CompleteResponseAsync(callback);
    }

    /// <summary>Sets the response timeout.</summary>
    public ServerResponse setTimeout(int msecs, Action? callback = null)
    {
        JsNumeric.RequireNonNegativeInt(msecs, nameof(msecs));
        if (callback is not null)
            once("timeout", callback);

        _timeoutTimer?.Dispose();
        if (msecs > 0)
        {
            _timeoutTimer = new Timer(_ =>
            {
                if (Volatile.Read(ref _terminal) == 0)
                    JsEventLoop.EnqueueReferenced(() =>
                    {
                        if (Volatile.Read(ref _terminal) == 0)
                            emit("timeout");
                    });
            }, null, msecs, System.Threading.Timeout.Infinite);
        }
        return this;
    }

    /// <summary>Starts native response headers in the ordered response pipeline.</summary>
    public void flushHeaders()
    {
        var start = EnsureHeadersStartedAsync();
        if (!start.IsCompletedSuccessfully)
            _ = ObserveHeadersStartAsync(start);
    }

    private void SetHeaderValues(string name, IReadOnlyList<string> values)
    {
        EnsureHeadersMutable();
        ValidateHeaderValues(name, values);
        _response.Headers[name] = new StringValues(values.ToArray());
    }

    private void AppendHeaderValues(string name, IReadOnlyList<string> values)
    {
        EnsureHeadersMutable();
        ValidateHeaderValues(name, values);
        foreach (var value in values)
            _response.Headers.Append(name, value);
    }

    private static void ValidateHeaderValues(string name, IReadOnlyList<string> values)
    {
        http.validateHeaderName(name);
        foreach (var value in values)
            http.validateHeaderValue(name, value);
    }

    private void ApplyHeaders(OutgoingHttpHeaders headers)
    {
        foreach (var name in headers.names())
            SetHeaderValues(name, headers.getAll(name));
    }

    private void EnsureHeadersMutable()
    {
        if (headersSent)
            throw new InvalidOperationException("Response headers have already been sent.");
    }

    private void MarkHeadersStarted()
    {
        if (Interlocked.CompareExchange(ref _headersStarted, 1, 0) != 0)
            throw new InvalidOperationException("Response headers have already been sent.");
    }

    private Task EnsureHeadersStartedAsync()
    {
        Interlocked.Exchange(ref _headersStarted, 1);
        lock (_headersStartLock)
            return _headersStartTask ??= _response.StartAsync(_response.HttpContext.RequestAborted);
    }

    private async Task WriteChunkAsync(object? chunk, string? encoding, Action callback)
    {
        try
        {
            await EnsureHeadersStartedAsync().ConfigureAwait(false);
            var bytes = chunk switch
            {
                string text => Buffer.from(text, encoding ?? "utf8").InternalMemory,
                Buffer buffer => buffer.InternalMemory,
                byte[] value => value.AsMemory(),
                _ => throw new ArgumentException(
                    "ServerResponse.write requires a string or binary chunk.",
                    nameof(chunk)),
            };
            await _response.Body.WriteAsync(
                bytes,
                _response.HttpContext.RequestAborted).ConfigureAwait(false);
            JsEventLoop.EnqueueReferenced(callback);
        }
        catch (Exception error)
        {
            EnqueueFailure(error, callback);
        }
    }

    private async Task CompleteResponseAsync(Action callback)
    {
        try
        {
            await EnsureHeadersStartedAsync().ConfigureAwait(false);
            await _response.CompleteAsync().ConfigureAwait(false);
            JsEventLoop.EnqueueReferenced(() =>
            {
                if (Interlocked.Exchange(ref _terminal, 1) != 0)
                    return;
                _timeoutTimer?.Dispose();
                _abortRegistration.Dispose();
                callback();
                _completion.TrySetResult();
                emit("close");
            });
        }
        catch (Exception error)
        {
            EnqueueFailure(error, callback);
        }
    }

    private async Task ObserveHeadersStartAsync(Task start)
    {
        try
        {
            await start.ConfigureAwait(false);
        }
        catch (Exception error)
        {
            EnqueueFailure(error);
        }
    }

    private void EnqueueFailure(Exception error, Action? stateCallback = null)
    {
        JsEventLoop.EnqueueReferenced(() =>
        {
            if (Interlocked.Exchange(ref _terminal, 1) != 0)
                return;
            _timeoutTimer?.Dispose();
            _abortRegistration.Dispose();
            _completion.TrySetException(error);
            base.destroy(error);
            stateCallback?.Invoke();
        });
    }

    private void AbortFromTransport()
    {
        JsEventLoop.EnqueueReferenced(() =>
        {
            if (Interlocked.Exchange(ref _terminal, 1) != 0)
                return;
            _timeoutTimer?.Dispose();
            _abortRegistration.Dispose();
            _completion.TrySetCanceled(_response.HttpContext.RequestAborted);
            base.destroy();
        });
    }
}
