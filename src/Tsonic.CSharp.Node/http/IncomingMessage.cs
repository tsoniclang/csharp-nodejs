using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
    private readonly string? _body;
    private bool _isServerSide;
    private bool _clientBodyEmitted;
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
    internal IncomingMessage(HttpResponseMessage response, string body)
    {
        _clientResponse = response;
        _body = body;
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
        complete = true;
        _timeoutTimer?.Dispose();
        emit("close");
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
                {
                    emit("timeout");
                }
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
        if (_isServerSide && _serverRequest != null)
        {
            using var reader = new StreamReader(_serverRequest.Body);
            var body = await reader.ReadToEndAsync();
            complete = true;
            _timeoutTimer?.Dispose();
            emit("end");
            return body;
        }
        else if (_body != null)
        {
            EmitBufferedClientBody();
            return _body;
        }

        return "";
    }

    /// <summary>
    /// Event handler for 'data' event.
    /// Note: In Node.js, this is an event. Here we provide a helper to read chunks.
    /// </summary>
    public void onData(Action<string> callback)
    {
        on("data", callback);
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

    internal void EmitBufferedClientBody()
    {
        if (_isServerSide || _clientBodyEmitted)
        {
            return;
        }

        _clientBodyEmitted = true;

        if (!string.IsNullOrEmpty(_body))
        {
            emit("data", _body);
        }

        complete = true;
        _timeoutTimer?.Dispose();
        emit("end");
        emit("close");
    }
}
