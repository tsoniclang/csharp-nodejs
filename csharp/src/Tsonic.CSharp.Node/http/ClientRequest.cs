using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Tsonic.CSharp.Js;
using Tsonic.CSharp.Node;

namespace Tsonic.CSharp.Node.Http;

/// <summary>
/// Implements Node.js http.ClientRequest.
/// Wraps HttpClient to provide Node.js-compatible API for making HTTP requests.
/// Extends EventEmitter to support events like 'response', 'error', 'timeout'.
/// </summary>
public partial class ClientRequest : EventEmitter
{
    private readonly HttpClient _httpClient;
    private readonly RequestOptions _options;
    private readonly HttpRequestMessage _request;
    private readonly bool _ownsHttpClient;
    private readonly MemoryStream _requestBody = new();
    private bool _aborted = false;
    private bool _ended = false;
    private readonly System.Threading.CancellationTokenSource _cancellation = new();
    private IncomingMessage? _response;
    private int _released;

    internal ClientRequest(HttpClient httpClient, RequestOptions options, Action<IncomingMessage>? callback, bool ownsHttpClient = false)
    {
        _httpClient = httpClient;
        _options = options;
        _ownsHttpClient = ownsHttpClient;
        // Build URL
        var protocol = options.protocol ?? "http:";
        var hostname = options.hostname ?? "localhost";
        var port = options.port;
        var path = options.path ?? "/";

        // Only include port in URL if non-default
        var url = port == 80 || port == 0
            ? $"{protocol}//{hostname}{path}"
            : $"{protocol}//{hostname}:{port}{path}";

        _request = new HttpRequestMessage(new HttpMethod(options.method), url);

        // Add headers
        if (options.headers != null)
        {
            foreach (var header in options.headers)
            {
                _request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        // Add basic auth if provided
        if (!string.IsNullOrEmpty(options.auth))
        {
            var authBytes = Encoding.UTF8.GetBytes(options.auth);
            var authBase64 = Convert.ToBase64String(authBytes);
            _request.Headers.TryAddWithoutValidation("Authorization", $"Basic {authBase64}");
        }

        // Register response callback if provided
        if (callback != null)
        {
            on("response", callback);
        }
    }

    /// <summary>
    /// Gets or sets the request path.
    /// </summary>
    public string path => _options.path ?? "/";

    /// <summary>
    /// Gets or sets the request method.
    /// </summary>
    public string method => _options.method;

    /// <summary>
    /// Gets or sets the request host.
    /// </summary>
    public string host => _options.hostname ?? "localhost";

    /// <summary>
    /// Gets or sets the request protocol.
    /// </summary>
    public string protocol => _options.protocol ?? "http:";

    /// <summary>
    /// Boolean indicating if the request has been aborted.
    /// </summary>
    public bool aborted => _aborted;

    /// <summary>
    /// Sets a single header value for the request.
    /// </summary>
    /// <param name="name">Header name.</param>
    /// <param name="value">Header value.</param>
    public void setHeader(string name, string value)
    {
        if (_ended)
            throw new InvalidOperationException("Cannot set headers after request has been sent");

        _request.Headers.Remove(name);
        _request.Headers.TryAddWithoutValidation(name, value);
    }

    /// <summary>
    /// Gets the value of a header.
    /// </summary>
    /// <param name="name">Header name.</param>
    /// <returns>Header value or null if not set.</returns>
    public string? getHeader(string name)
    {
        if (_request.Headers.TryGetValues(name, out var values))
            return string.Join(", ", values);
        return null;
    }

    /// <summary>
    /// Returns an array containing the unique names of the current outgoing headers.
    /// </summary>
    /// <returns>Array of header names.</returns>
    public string[] getHeaderNames()
    {
        var names = new System.Collections.Generic.List<string>();
        foreach (var header in _request.Headers)
        {
            names.Add(header.Key);
        }
        return names.ToArray();
    }

    /// <summary>
    /// Removes a header that's already been added to the request.
    /// </summary>
    /// <param name="name">Header name.</param>
    public void removeHeader(string name)
    {
        if (_ended)
            throw new InvalidOperationException("Cannot remove headers after request has been sent");

        _request.Headers.Remove(name);
    }

    /// <summary>
    /// Writes a chunk of data to the request body.
    /// </summary>
    /// <param name="chunk">The data to write.</param>
    /// <param name="encoding">Optional encoding (ignored, always UTF-8).</param>
    /// <param name="callback">Optional callback when chunk is flushed.</param>
    /// <returns>True if entire data was flushed successfully.</returns>
    public bool write(string chunk, string? encoding = null, Action? callback = null)
    {
        if (_ended)
            throw new InvalidOperationException("Cannot write after request has been sent");

        var bytes = Buffer.from(chunk, encoding ?? "utf8").InternalMemory;
        _requestBody.Write(bytes.Span);
        if (callback != null)
            Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(callback);
        return true;
    }

    /// <summary>Writes a binary chunk to the request body.</summary>
    public bool write(Buffer chunk, Action? callback = null)
    {
        if (_ended)
            throw new InvalidOperationException("Cannot write after request has been sent");

        var bytes = chunk.InternalMemory;
        _requestBody.Write(bytes.Span);
        if (callback != null)
            Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(callback);
        return true;
    }

    /// <summary>
    /// Finishes sending the request.
    /// If any part of the body is unsent, it will flush them to the stream.
    /// </summary>
    /// <param name="chunk">Optional final chunk to send.</param>
    /// <param name="encoding">Optional encoding (ignored, always UTF-8).</param>
    /// <param name="callback">Optional callback when request is sent.</param>
    public async Task end(string? chunk = null, string? encoding = null, Action? callback = null)
    {
        if (_ended)
            return;

        if (chunk != null)
        {
            write(chunk, encoding);
        }

        _ended = true;
        ProcessKeepAlive.Acquire();
        var transferred = false;
        try
        {
            // Set request body if present
            if (_requestBody.Length > 0)
            {
                _requestBody.Position = 0;
                _request.Content = new StreamContent(_requestBody);
            }

            // Apply timeout if specified
            if (_options.timeout.HasValue)
            {
                _cancellation.CancelAfter(_options.timeout.Value);
            }
            var response = await _httpClient.SendAsync(_request, HttpCompletionOption.ResponseHeadersRead, _cancellation.Token);
            _response = new IncomingMessage(response, _cancellation.Token, ReleaseRequest);
            transferred = true;
            var incomingMessage = _response;
            Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() =>
            {
                try { emit("response", incomingMessage); }
                catch { incomingMessage.destroy(); throw; }
                Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(incomingMessage.StartClientBody);
            });

            if (callback != null)
                Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(callback);
        }
        catch (TaskCanceledException)
        {
            _response?.destroy();
            Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() =>
            {
                emit("timeout");
                emit("error", new TimeoutException("Request timeout"));
            });
        }
        catch (Exception ex)
        {
            _response?.destroy();
            Tsonic.CSharp.Js.JsEventLoop.EnqueueReferenced(() => emit("error", ex));
        }
        finally
        {
            if (!transferred) ReleaseRequest();
            ProcessKeepAlive.Release();
        }
    }

    /// <summary>Finishes the request after writing a final binary chunk.</summary>
    public Task end(Buffer chunk, Action? callback = null)
    {
        if (_ended)
            return Task.CompletedTask;

        write(chunk);
        return end(callback: callback);
    }

    private void ReleaseRequest()
    {
        lock (_cancellation)
        {
            if (_released != 0) return;
            _released = 1;
            _request.Dispose();
            _requestBody.Dispose();
            _cancellation.Dispose();
            if (_ownsHttpClient) _httpClient.Dispose();
        }
    }

    /// <summary>
    /// Aborts the ongoing request.
    /// </summary>
    public void abort()
    {
        lock (_cancellation)
        {
            if (_aborted || _released != 0) return;
            _aborted = true;
            _cancellation.Cancel();
        }
        _response?.destroy();
        emit("abort");
    }

    /// <summary>
    /// Sets the timeout value in milliseconds for the request.
    /// </summary>
    /// <param name="msecs">Timeout in milliseconds.</param>
    /// <param name="callback">Optional callback for timeout event.</param>
    /// <returns>The ClientRequest instance.</returns>
    public ClientRequest setTimeout(int msecs, Action? callback = null)
    {
        _options.timeout = msecs;

        if (callback != null)
        {
            once("timeout", callback);
        }

        return this;
    }
}
