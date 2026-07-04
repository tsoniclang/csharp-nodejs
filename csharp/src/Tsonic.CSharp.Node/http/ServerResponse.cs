using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tsonic.CSharp.Node;

namespace Tsonic.CSharp.Node.Http;

/// <summary>
/// Implements Node.js http.ServerResponse.
/// Wraps ASP.NET Core HttpResponse to provide Node.js-compatible API.
/// Extends EventEmitter to support events like 'finish', 'close'.
/// </summary>
public partial class ServerResponse : EventEmitter
{
    private readonly HttpResponse _response;
    private bool _headersSent = false;
    private bool _finished = false;
    private Timer? _timeoutTimer;

    internal ServerResponse(HttpResponse response)
    {
        _response = response;
    }

    /// <summary>
    /// Gets or sets the HTTP status code that will be sent to the client.
    /// </summary>
    public int statusCode
    {
        get => _response.StatusCode;
        set
        {
            if (_headersSent)
                throw new InvalidOperationException("Cannot set status code after headers have been sent");
            _response.StatusCode = value;
        }
    }

    /// <summary>
    /// Gets or sets the HTTP status message that will be sent to the client.
    /// Note: In HTTP/2, status messages are ignored.
    /// </summary>
    public string statusMessage { get; set; } = "";

    /// <summary>
    /// Boolean indicating if headers were sent.
    /// Read-only.
    /// </summary>
    public bool headersSent => _headersSent;

    /// <summary>
    /// Boolean indicating if the response has completed.
    /// </summary>
    public bool finished => _finished;

    /// <summary>
    /// Sends a response header to the request.
    /// Must be called before end() or write().
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="statusMessage">Optional status message (ignored in HTTP/2).</param>
    /// <param name="headers">Optional headers object.</param>
    /// <returns>The ServerResponse instance for chaining.</returns>
    public ServerResponse writeHead(int statusCode, string? statusMessage = null, Dictionary<string, string>? headers = null)
    {
        if (_headersSent)
            throw new InvalidOperationException("Headers already sent");

        _response.StatusCode = statusCode;

        if (statusMessage != null)
            this.statusMessage = statusMessage;

        if (headers != null)
        {
            foreach (var header in headers)
            {
                _response.Headers[header.Key] = header.Value;
            }
        }

        _headersSent = true;
        return this;
    }

    /// <summary>
    /// Sends a response header to the request (overload with just headers).
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="headers">Headers object.</param>
    /// <returns>The ServerResponse instance for chaining.</returns>
    public ServerResponse writeHead(int statusCode, Dictionary<string, string> headers)
    {
        return writeHead(statusCode, null, headers);
    }

    /// <summary>
    /// Sets a single header value for implicit headers.
    /// </summary>
    /// <param name="name">Header name.</param>
    /// <param name="value">Header value.</param>
    /// <returns>The ServerResponse instance for chaining.</returns>
    public ServerResponse setHeader(string name, string value)
    {
        if (_headersSent)
            throw new InvalidOperationException("Headers already sent");

        _response.Headers[name] = value;
        return this;
    }

    /// <summary>
    /// Gets the value of a header that's already been queued but not sent.
    /// </summary>
    /// <param name="name">Header name.</param>
    /// <returns>Header value or null if not set.</returns>
    public string? getHeader(string name)
    {
        if (_response.Headers.TryGetValue(name, out var value))
            return value.ToString();
        return null;
    }

    /// <summary>
    /// Returns an array containing the unique names of the current outgoing headers.
    /// </summary>
    /// <returns>Array of header names.</returns>
    public string[] getHeaderNames()
    {
        var names = new List<string>();
        foreach (var header in _response.Headers)
        {
            names.Add(header.Key);
        }
        return names.ToArray();
    }

    /// <summary>
    /// Returns a shallow copy of the current outgoing headers.
    /// </summary>
    /// <returns>Dictionary of headers.</returns>
    public Dictionary<string, string> getHeaders()
    {
        var headers = new Dictionary<string, string>();
        foreach (var header in _response.Headers)
        {
            headers[header.Key] = header.Value.ToString();
        }
        return headers;
    }

    /// <summary>
    /// Returns true if the header identified by name is currently set in the outgoing headers.
    /// </summary>
    /// <param name="name">Header name.</param>
    /// <returns>True if header exists.</returns>
    public bool hasHeader(string name)
    {
        return _response.Headers.ContainsKey(name);
    }

    /// <summary>
    /// Removes a header that's queued for implicit sending.
    /// </summary>
    /// <param name="name">Header name.</param>
    public void removeHeader(string name)
    {
        if (_headersSent)
            throw new InvalidOperationException("Headers already sent");

        _response.Headers.Remove(name);
    }

    /// <summary>
    /// Sends a chunk of the response body.
    /// Writes synchronously using blocking wait to match Node.js semantics.
    /// </summary>
    /// <param name="chunk">The data to write.</param>
    /// <param name="encoding">Optional encoding (ignored, always UTF-8).</param>
    /// <param name="callback">Optional callback when chunk is flushed.</param>
    /// <returns>True if entire data was flushed successfully.</returns>
    public bool write(string chunk, string? encoding = null, Action? callback = null)
    {
        if (!_headersSent)
        {
            _headersSent = true;
        }

        // Write synchronously - use Task.Run to avoid sync-over-async deadlock
        Task.Run(async () => await _response.WriteAsync(chunk)).GetAwaiter().GetResult();
        callback?.Invoke();
        return true;
    }

    /// <summary>
    /// Sends a chunk of the response body from a Buffer.
    /// </summary>
    public bool write(Buffer chunk, Action? callback = null)
    {
        if (!_headersSent)
        {
            _headersSent = true;
        }

        Task.Run(async () => await _response.Body.WriteAsync(chunk.InternalData)).GetAwaiter().GetResult();
        callback?.Invoke();
        return true;
    }

    /// <summary>
    /// Sends a chunk of the response body from a byte array.
    /// </summary>
    public bool write(byte[] chunk, Action? callback = null)
    {
        if (!_headersSent)
        {
            _headersSent = true;
        }

        Task.Run(async () => await _response.Body.WriteAsync(chunk)).GetAwaiter().GetResult();
        callback?.Invoke();
        return true;
    }

    /// <summary>
    /// Signals that all response headers and body have been sent.
    /// Node.js idiomatic: synchronous from caller's perspective, returns this for chaining.
    /// </summary>
    /// <returns>This response for chaining.</returns>
    public ServerResponse end()
    {
        _finished = true;
        _timeoutTimer?.Dispose();
        emit("finish");
        return this;
    }

    /// <summary>
    /// Signals response completion with an optional callback and no payload.
    /// </summary>
    public ServerResponse end(Action? callback)
    {
        var response = end();
        callback?.Invoke();
        return response;
    }

    /// <summary>
    /// Signals that all response headers and body have been sent.
    /// Node.js idiomatic: synchronous from caller's perspective, returns this for chaining.
    /// </summary>
    /// <param name="chunk">Final chunk to send.</param>
    /// <param name="encoding">Optional encoding (ignored, always UTF-8).</param>
    /// <param name="callback">Optional callback when response is finished.</param>
    /// <returns>This response for chaining.</returns>
    public ServerResponse end(string chunk, string? encoding = null, Action? callback = null)
    {
        // Write synchronously - use Task.Run to avoid sync-over-async deadlock
        // Server will call CompleteAsync after emit returns
        Task.Run(async () => await _response.WriteAsync(chunk)).GetAwaiter().GetResult();
        var response = end();
        callback?.Invoke();
        return response;
    }

    /// <summary>
    /// Signals response completion with an optional Buffer payload.
    /// </summary>
    public ServerResponse end(Buffer chunk, Action? callback = null)
    {
        Task.Run(async () => await _response.Body.WriteAsync(chunk.InternalData)).GetAwaiter().GetResult();
        var response = end();
        callback?.Invoke();
        return response;
    }

    /// <summary>
    /// Signals response completion with an optional byte-array payload.
    /// </summary>
    public ServerResponse end(byte[] chunk, Action? callback = null)
    {
        Task.Run(async () => await _response.Body.WriteAsync(chunk)).GetAwaiter().GetResult();
        var response = end();
        callback?.Invoke();
        return response;
    }

    /// <summary>
    /// Sets the timeout value in milliseconds for the response.
    /// </summary>
    /// <param name="msecs">Timeout in milliseconds.</param>
    /// <param name="callback">Optional callback for timeout event.</param>
    /// <returns>The ServerResponse instance.</returns>
    public ServerResponse setTimeout(int msecs, Action? callback = null)
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
                if (!_finished)
                {
                    emit("timeout");
                }
            }, null, msecs, System.Threading.Timeout.Infinite);
        }
        return this;
    }

    /// <summary>
    /// Flushes the response headers.
    /// </summary>
    public void flushHeaders()
    {
        if (!_headersSent)
        {
            _headersSent = true;
            _ = _response.StartAsync();
        }
    }
}
