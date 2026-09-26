using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tsonic.CSharp.Js;
using Tsonic.CSharp.Node;

namespace Tsonic.CSharp.Node.Http;

/// <summary>A single-consumer streaming HTTP request or client response.</summary>
public partial class IncomingMessage : Readable
{
    private readonly HttpRequest? _serverRequest;
    private readonly HttpResponseMessage? _clientResponse;
    private readonly bool _isServerSide;
    private readonly CancellationTokenSource _bodyCancellation;
    private readonly Action? _releaseRequest;
    private readonly Socket _socket;
    private int _bodyStarted;
    private int _materializerClaimed;
    private int _released;
    private int _terminal;
    private Timer? _timeoutTimer;

    internal IncomingMessage(HttpRequest request)
    {
        _serverRequest = request;
        _isServerSide = true;
        _bodyCancellation = CancellationTokenSource.CreateLinkedTokenSource(request.HttpContext.RequestAborted);
        headers = new IncomingHttpHeaders(request.Headers);
        _socket = Socket.FromHttpConnection(
            request.HttpContext.Connection,
            request.HttpContext.Abort,
            request.HttpContext.RequestAborted);
    }

    internal IncomingMessage(
        HttpResponseMessage response,
        CancellationToken cancellation,
        Action releaseRequest)
    {
        _clientResponse = response;
        _releaseRequest = releaseRequest;
        _bodyCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        ProcessKeepAlive.Acquire();
        _isServerSide = false;
        headers = new IncomingHttpHeaders(
            response.Headers.Select(header =>
                new KeyValuePair<string, IEnumerable<string>>(header.Key, header.Value))
            .Concat(response.Content.Headers.Select(header =>
                new KeyValuePair<string, IEnumerable<string>>(header.Key, header.Value))));
        _socket = Socket.FromHttpResponse(response);
    }

    /// <summary>Request method, or native absence for a client response.</summary>
    public string? method => _isServerSide ? _serverRequest?.Method : null;

    /// <summary>Request URL, or native absence for a client response.</summary>
    public string? url => _isServerSide
        ? _serverRequest?.Path + _serverRequest?.QueryString
        : null;

    /// <summary>HTTP protocol version.</summary>
    public string httpVersion => _isServerSide
        ? _serverRequest?.Protocol.Replace("HTTP/", "", StringComparison.Ordinal) ?? "1.1"
        : _clientResponse?.Version.ToString() ?? "1.1";

    /// <summary>Response status code, or native absence for a server request.</summary>
    public int? statusCode => _isServerSide ? null : (int?)_clientResponse?.StatusCode;

    /// <summary>Response reason phrase, or native absence for a server request.</summary>
    public string? statusMessage => _isServerSide ? null : _clientResponse?.ReasonPhrase;

    /// <summary>Case-insensitive live/snapshotted header access.</summary>
    public IncomingHttpHeaders headers { get; }

    /// <summary>Exact repeated header values through the canonical header carrier.</summary>
    public IncomingHttpHeaders headersDistinct => headers;

    /// <summary>True only after the complete framed body has been received.</summary>
    public bool complete { get; private set; }

    /// <summary>True when local destruction or transport cancellation interrupts the body.</summary>
    public bool aborted { get; private set; }

    /// <summary>The request's transport endpoint metadata.</summary>
    public Socket socket => _socket;

    /// <summary>Destroys the request body and aborts its native transport.</summary>
    public override void destroy(Exception? error = null)
    {
        if (Interlocked.Exchange(ref _terminal, 1) != 0)
            return;

        aborted = !complete;
        _bodyCancellation.Cancel();
        if (_serverRequest is not null)
            _serverRequest.HttpContext.Abort();
        ReleaseNative();
        if (aborted)
            emit("aborted");
        base.destroy(error);
    }

    /// <summary>Destroys the request and returns it for fluent source code.</summary>
    public new IncomingMessage destroyChain(Exception? error = null)
    {
        destroy(error);
        return this;
    }

    /// <summary>Sets a request timeout.</summary>
    public IncomingMessage setTimeout(int msecs, Action? callback = null)
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

    /// <summary>Materializes the body as UTF-8 text through the canonical readable stream.</summary>
    public async Task<string> readAll()
    {
        var buffer = await readAllBuffer().ConfigureAwait(false);
        return buffer.toString("utf8");
    }

    /// <summary>Materializes the body through the canonical readable stream.</summary>
    public Task<Buffer> readAllBuffer()
    {
        if (Interlocked.CompareExchange(ref _materializerClaimed, 1, 0) != 0)
            throw new InvalidOperationException("The message body already has a materializing consumer");

        var completion = new TaskCompletionSource<Buffer>(TaskCreationOptions.RunContinuationsAsynchronously);
        var chunks = new List<Buffer>();
        Action<Buffer>? onData = null;
        Action? onEnd = null;
        Action<Exception>? onError = null;
        Action? onAborted = null;

        void Detach()
        {
            if (onData is not null)
                off("data", onData);
            if (onEnd is not null)
                off("end", onEnd);
            if (onError is not null)
                off("error", onError);
            if (onAborted is not null)
                off("aborted", onAborted);
        }

        onData = chunk => chunks.Add(chunk);
        onEnd = () =>
        {
            Detach();
            completion.TrySetResult(Buffer.concat(chunks.ToArray()));
        };
        onError = error =>
        {
            Detach();
            completion.TrySetException(error);
        };
        onAborted = () =>
        {
            Detach();
            completion.TrySetException(new IOException("The HTTP message was aborted"));
        };
        once("end", onEnd);
        once("error", onError);
        once("aborted", onAborted);
        on("data", onData);
        return completion.Task;
    }

    /// <summary>Registers a typed body-data listener.</summary>
    public void onData(Action<Buffer> callback) => on("data", callback);

    /// <summary>Registers a typed body-end listener.</summary>
    public void onEnd(Action callback) => on("end", callback);

    /// <summary>Registers a typed close listener.</summary>
    public void onClose(Action callback) => on("close", callback);

    internal void StartClientBody() => StartBodyPump();

    /// <inheritdoc />
    protected override void _read(int size)
    {
        _ = size;
        StartBodyPump();
    }

    private void StartBodyPump()
    {
        if (Volatile.Read(ref _terminal) != 0 ||
            Interlocked.CompareExchange(ref _bodyStarted, 1, 0) != 0)
            return;
        _ = PumpBodyAsync();
    }

    private async Task PumpBodyAsync()
    {
        try
        {
            var stream = await BodyStreamAsync().ConfigureAwait(false);
            while (true)
            {
                var bytes = new byte[16 * 1024];
                var count = await stream.ReadAsync(
                    bytes.AsMemory(),
                    _bodyCancellation.Token).ConfigureAwait(false);
                if (count == 0)
                    break;

                var chunk = Buffer.TakeOwnership(bytes.AsMemory(0, count));
                var accepted = await PublishChunkAsync(chunk).ConfigureAwait(false);
                if (!accepted)
                    await WaitForReadCapacityAsync(_bodyCancellation.Token).ConfigureAwait(false);
            }

            complete = true;
            await PublishEndAsync().ConfigureAwait(false);
            FinishNormally();
        }
        catch (OperationCanceledException) when (_bodyCancellation.IsCancellationRequested)
        {
            AbortFromTransport();
        }
        catch (Exception error)
        {
            FailFromTransport(error);
        }
    }

    private Task<System.IO.Stream> BodyStreamAsync() => _serverRequest is not null
        ? Task.FromResult(_serverRequest.Body)
        : _clientResponse!.Content.ReadAsStreamAsync(_bodyCancellation.Token);

    private Task<bool> PublishChunkAsync(Buffer chunk)
    {
        var published = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        JsEventLoop.EnqueueReferenced(() =>
        {
            try
            {
                published.TrySetResult(Volatile.Read(ref _terminal) == 0 && push(chunk));
            }
            catch (Exception error)
            {
                published.TrySetException(error);
            }
        });
        return published.Task;
    }

    private Task PublishEndAsync()
    {
        var published = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        JsEventLoop.EnqueueReferenced(() =>
        {
            try
            {
                if (Volatile.Read(ref _terminal) == 0)
                    push(null);
                published.TrySetResult();
            }
            catch (Exception error)
            {
                published.TrySetException(error);
            }
        });
        return published.Task;
    }

    private void FinishNormally()
    {
        if (Interlocked.Exchange(ref _terminal, 1) != 0)
            return;
        ReleaseNative();
        JsEventLoop.EnqueueReferenced(() => emit("close"));
    }

    internal void CloseAfterResponse()
    {
        if (Interlocked.Exchange(ref _terminal, 1) != 0)
            return;
        _bodyCancellation.Cancel();
        ReleaseNative();
        JsEventLoop.EnqueueReferenced(() =>
        {
            if (!destroyed)
                base.destroy();
        });
    }

    private void AbortFromTransport()
    {
        if (Interlocked.Exchange(ref _terminal, 1) != 0)
            return;
        aborted = !complete;
        ReleaseNative();
        JsEventLoop.EnqueueReferenced(() =>
        {
            if (aborted)
                emit("aborted");
            base.destroy();
        });
    }

    private void FailFromTransport(Exception error)
    {
        if (Interlocked.Exchange(ref _terminal, 1) != 0)
            return;
        aborted = !complete;
        ReleaseNative();
        JsEventLoop.EnqueueReferenced(() =>
        {
            if (aborted)
                emit("aborted");
            base.destroy(error);
        });
    }

    private void ReleaseNative()
    {
        if (Interlocked.Exchange(ref _released, 1) != 0)
            return;
        _timeoutTimer?.Dispose();
        _bodyCancellation.Dispose();
        _clientResponse?.Dispose();
        _releaseRequest?.Invoke();
        if (!_isServerSide)
            ProcessKeepAlive.Release();
    }
}
