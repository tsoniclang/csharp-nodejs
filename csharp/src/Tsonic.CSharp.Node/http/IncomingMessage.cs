using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tsonic.CSharp.Node;

namespace Tsonic.CSharp.Node.Http;

/// <summary>
/// Implements Node.js http.IncomingMessage.
/// Represents an incoming HTTP request (server-side) or response (client-side).
/// Extends EventEmitter and implements readable stream interface.
/// </summary>
public partial class IncomingMessage : EventEmitter
{
    private readonly HttpRequest? _serverRequest;
    private readonly HttpResponseMessage? _clientResponse;
    private bool _isServerSide;
    private readonly CancellationToken _cancellation;
    private readonly Action? _releaseRequest;
    private int _bodyClaimed;
    private int _released;
    private Timer? _timeoutTimer;

    // Server-side constructor
    internal IncomingMessage(HttpRequest request)
    {
        _serverRequest = request;
        _isServerSide = true;

        // Read headers
        headers = new Dictionary<string, string>();
        foreach (var header in request.Headers)
        {
            headers[header.Key.ToLowerInvariant()] = header.Value.ToString();
        }
    }

    // Client-side constructor
    internal IncomingMessage(HttpResponseMessage response, CancellationToken cancellation, Action releaseRequest)
    {
        _clientResponse = response;
        _cancellation = cancellation;
        _releaseRequest = releaseRequest;
        ProcessKeepAlive.Acquire();
        _isServerSide = false;

        // Read headers
        headers = new Dictionary<string, string>();
        foreach (var header in response.Headers)
        {
            headers[header.Key.ToLowerInvariant()] = string.Join(", ", header.Value);
        }
        foreach (var header in response.Content.Headers)
        {
            headers[header.Key.ToLowerInvariant()] = string.Join(", ", header.Value);
        }
    }

    /// <summary>
    /// Request method (server-side) or null (client-side).
    /// </summary>
    public string? method => _isServerSide ? _serverRequest?.Method : null;

    /// <summary>
    /// Request URL (server-side) or null (client-side).
    /// </summary>
    public string? url => _isServerSide ? _serverRequest?.Path + _serverRequest?.QueryString : null;

    /// <summary>
    /// HTTP version sent by the client.
    /// </summary>
    public string httpVersion
    {
        get
        {
            if (_isServerSide)
            {
                return _serverRequest?.Protocol.Replace("HTTP/", "") ?? "1.1";
            }
            else
            {
                return _clientResponse?.Version.ToString() ?? "1.1";
            }
        }
    }

    /// <summary>
    /// Response status code (client-side) or null (server-side).
    /// </summary>
    public int? statusCode => _isServerSide ? null : (int?)_clientResponse?.StatusCode;

    /// <summary>
    /// Response status message (client-side) or null (server-side).
    /// </summary>
    public string? statusMessage => _isServerSide ? null : _clientResponse?.ReasonPhrase;

    /// <summary>
    /// Request/response headers object.
    /// </summary>
    public Dictionary<string, string> headers { get; }

    /// <summary>
    /// Indicates that the underlying connection was closed.
    /// </summary>
    public bool complete { get; private set; } = false;

    /// <summary>
    /// Calls destroy() on the socket that received the IncomingMessage.
    /// </summary>
    public void destroy()
    {
        Finish(false);
    }

    /// <summary>
    /// Sets the timeout value in milliseconds for the incoming message.
    /// </summary>
    /// <param name="msecs">Timeout in milliseconds.</param>
    /// <param name="callback">Optional callback for timeout event.</param>
    /// <returns>The IncomingMessage instance.</returns>
    public IncomingMessage setTimeout(int msecs, Action? callback = null)
    {
        JsNumeric.RequireNonNegativeInt(msecs, nameof(msecs));

        if (callback != null)
        {
            once("timeout", callback);
        }

        _timeoutTimer?.Dispose();
        if (msecs > 0)
        {
            _timeoutTimer = new Timer(_ =>
            {
                if (!complete)
                    Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() =>
                    {
                        if (!complete)
                            emit("timeout");
                    });
            }, null, msecs, System.Threading.Timeout.Infinite);
        }
        return this;
    }

    // Stream-like interface for reading body

    /// <summary>
    /// Reads the entire body as a string (simplified implementation).
    /// In a full implementation, this would be a streaming interface.
    /// </summary>
    /// <returns>The body content as a string.</returns>
    public async Task<string> readAll()
    {
        ClaimBody();
        try
        {
            var stream = await BodyStream();
            using var reader = new StreamReader(stream, Encoding.UTF8, true, 4096, leaveOpen: true);
            var body = await reader.ReadToEndAsync(_cancellation);
            Finish(true);
            return body;
        }
        catch { Finish(false); throw; }
    }

    /// <summary>
    /// Event handler for 'data' event.
    /// Note: In Node.js, this is an event. Here we provide a helper to read chunks.
    /// </summary>
    public void onData(Action<Buffer> callback)
    {
        on("data", callback);
    }

    /// <summary>Reads the complete message body into a binary buffer.</summary>
    public async Task<Buffer> readAllBuffer()
    {
        ClaimBody();
        try
        {
            using var output = new MemoryStream();
            var stream = await BodyStream();
            await stream.CopyToAsync(output, _cancellation);
            if (!output.TryGetBuffer(out var bytes)) throw new InvalidOperationException("Owned body storage is not accessible");
            var result = Buffer.TakeOwnership(bytes.AsMemory());
            Finish(true);
            return result;
        }
        catch { Finish(false); throw; }
    }

    /// <summary>
    /// Event handler for 'end' event.
    /// </summary>
    public void onEnd(Action callback)
    {
        on("end", callback);
    }

    /// <summary>
    /// Event handler for 'close' event.
    /// </summary>
    public void onClose(Action callback)
    {
        on("close", callback);
    }

    private void ClaimBody()
    {
        if (Interlocked.CompareExchange(ref _bodyClaimed, 1, 0) != 0 || Volatile.Read(ref _released) != 0)
            throw new InvalidOperationException("The message body already has a consumer");
    }

    private Task<Stream> BodyStream() => _serverRequest is not null
        ? Task.FromResult(_serverRequest.Body)
        : _clientResponse!.Content.ReadAsStreamAsync(_cancellation);

    internal void StartClientBody()
    {
        if (_isServerSide || Volatile.Read(ref _released) != 0 ||
            Interlocked.CompareExchange(ref _bodyClaimed, 1, 0) != 0) return;
        _ = StreamClientBody();
    }

    private async Task StreamClientBody()
    {
        try
        {
            var stream = await BodyStream();
            while (Volatile.Read(ref _released) == 0)
            {
                var bytes = new byte[16 * 1024];
                var count = await stream.ReadAsync(bytes.AsMemory(), _cancellation);
                if (count == 0) break;
                var chunk = Buffer.TakeOwnership(bytes.AsMemory(0, count));
                var delivered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() =>
                {
                    try { if (Volatile.Read(ref _released) == 0) emit("data", chunk); delivered.SetResult(); }
                    catch (Exception error) { delivered.SetException(error); }
                });
                await delivered.Task;
            }
            Finish(true);
        }
        catch (Exception error)
        {
            if (Volatile.Read(ref _released) == 0)
                Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => emit("error", error));
            Finish(false);
        }
    }

    private void Finish(bool ended)
    {
        if (Interlocked.Exchange(ref _released, 1) != 0) return;
        complete = ended;
        _timeoutTimer?.Dispose();
        try
        {
            _clientResponse?.Dispose();
            _releaseRequest?.Invoke();
            Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() =>
            {
                if (ended) emit("end");
                emit("close");
            });
        }
        finally { if (!_isServerSide) ProcessKeepAlive.Release(); }
    }
}
