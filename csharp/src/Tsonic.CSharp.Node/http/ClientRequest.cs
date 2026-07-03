using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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
    private readonly StringBuilder _requestBody = new();
    private Action<IncomingMessage>? _responseCallback;
    private bool _aborted = false;
    private bool _ended = false;

    internal ClientRequest(HttpClient httpClient, RequestOptions options, Action<IncomingMessage>? callback)
    {
        _httpClient = httpClient;
        _options = options;
        _responseCallback = callback;

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

        _requestBody.Append(chunk);
        callback?.Invoke();
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

        try
        {
            // Set request body if present
            if (_requestBody.Length > 0)
            {
                _request.Content = new StringContent(_requestBody.ToString(), Encoding.UTF8, "text/plain");
            }

            // Apply timeout if specified
            if (_options.timeout.HasValue)
            {
                using var cts = new System.Threading.CancellationTokenSource(_options.timeout.Value);
                var response = await _httpClient.SendAsync(_request, cts.Token);
                await HandleResponse(response);
            }
            else
            {
                var response = await _httpClient.SendAsync(_request);
                await HandleResponse(response);
            }

            callback?.Invoke();
        }
        catch (TaskCanceledException)
        {
            emit("timeout");
            emit("error", new TimeoutException("Request timeout"));
        }
        catch (Exception ex)
        {
            emit("error", ex);
        }
    }

    private async Task HandleResponse(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        var incomingMessage = new IncomingMessage(response, body);

        emit("response", incomingMessage);

        _ = BackgroundDispatch.RunAsync(async () =>
        {
            await Task.Yield();
            incomingMessage.EmitBufferedClientBody();
        }, "Tsonic.CSharp.Node.Http.ClientRequest.ResponseBody");
    }

    /// <summary>
    /// Aborts the ongoing request.
    /// </summary>
    public void abort()
    {
        if (_aborted)
            return;

        _aborted = true;
        _request.Dispose();
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
