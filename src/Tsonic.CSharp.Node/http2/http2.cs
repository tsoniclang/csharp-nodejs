using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tsonic.CSharp.Node.Http;

namespace Tsonic.CSharp.Node.Http2;

#pragma warning disable CS1591

public static partial class http2
{
    public static readonly Dictionary<string, int> constants = new()
    {
        ["NGHTTP2_SESSION_SERVER"] = 0,
        ["NGHTTP2_SESSION_CLIENT"] = 1,
        ["HTTP2_HEADER_METHOD"] = 0,
        ["HTTP2_HEADER_PATH"] = 1,
        ["HTTP2_HEADER_STATUS"] = 2,
        ["HTTP2_HEADER_AUTHORITY"] = 3,
        ["HTTP2_HEADER_CONTENT_TYPE"] = 4,
        ["HTTP2_HEADER_CONTENT_LENGTH"] = 5,
        ["HTTP2_METHOD_GET"] = 10,
        ["HTTP2_METHOD_POST"] = 11,
        ["HTTP_STATUS_OK"] = 200,
        ["HTTP_STATUS_NOT_FOUND"] = 404,
        ["NGHTTP2_NO_ERROR"] = 0,
        ["NGHTTP2_PROTOCOL_ERROR"] = 1,
        ["NGHTTP2_CANCEL"] = 8,
        ["NGHTTP2_REFUSED_STREAM"] = 7
    };

    public const string HTTP2_HEADER_METHOD = ":method";
    public const string HTTP2_HEADER_PATH = ":path";
    public const string HTTP2_HEADER_STATUS = ":status";
    public const string HTTP2_HEADER_AUTHORITY = ":authority";
    public const string HTTP2_HEADER_CONTENT_TYPE = "content-type";
    public const string HTTP2_HEADER_CONTENT_LENGTH = "content-length";
    public const string HTTP2_HEADER_SCHEME = ":scheme";
    public const string HTTP2_HEADER_ACCEPT = "accept";
    public const string HTTP2_HEADER_USER_AGENT = "user-agent";
    public const string HTTP2_HEADER_HOST = "host";
    public const string HTTP2_HEADER_COOKIE = "cookie";
    public const string HTTP2_HEADER_SET_COOKIE = "set-cookie";
    public const string HTTP2_HEADER_LOCATION = "location";
    public const string HTTP2_HEADER_DATE = "date";
    public const string HTTP2_METHOD_GET = "GET";
    public const string HTTP2_METHOD_POST = "POST";
    public const string HTTP2_METHOD_PUT = "PUT";
    public const string HTTP2_METHOD_DELETE = "DELETE";
    public const string HTTP2_METHOD_PATCH = "PATCH";
    public const string HTTP2_METHOD_HEAD = "HEAD";
    public const string HTTP2_METHOD_OPTIONS = "OPTIONS";
    public const int HTTP_STATUS_OK = 200;
    public const int HTTP_STATUS_CREATED = 201;
    public const int HTTP_STATUS_NO_CONTENT = 204;
    public const int HTTP_STATUS_BAD_REQUEST = 400;
    public const int HTTP_STATUS_UNAUTHORIZED = 401;
    public const int HTTP_STATUS_FORBIDDEN = 403;
    public const int HTTP_STATUS_NOT_FOUND = 404;
    public const int HTTP_STATUS_METHOD_NOT_ALLOWED = 405;
    public const int HTTP_STATUS_CONFLICT = 409;
    public const int HTTP_STATUS_TOO_MANY_REQUESTS = 429;
    public const int HTTP_STATUS_INTERNAL_SERVER_ERROR = 500;
    public const int HTTP_STATUS_NOT_IMPLEMENTED = 501;
    public const int HTTP_STATUS_BAD_GATEWAY = 502;
    public const int HTTP_STATUS_SERVICE_UNAVAILABLE = 503;
    public const int HTTP_STATUS_GATEWAY_TIMEOUT = 504;
    public const int NGHTTP2_NO_ERROR = 0;
    public const int NGHTTP2_PROTOCOL_ERROR = 1;
    public const int NGHTTP2_INTERNAL_ERROR = 2;
    public const int NGHTTP2_FLOW_CONTROL_ERROR = 3;
    public const int NGHTTP2_SETTINGS_TIMEOUT = 4;
    public const int NGHTTP2_STREAM_CLOSED = 5;
    public const int NGHTTP2_FRAME_SIZE_ERROR = 6;
    public const int NGHTTP2_REFUSED_STREAM = 7;
    public const int NGHTTP2_CANCEL = 8;
    public const int NGHTTP2_COMPRESSION_ERROR = 9;
    public const int NGHTTP2_CONNECT_ERROR = 10;
    public const int NGHTTP2_ENHANCE_YOUR_CALM = 11;
    public const int NGHTTP2_INADEQUATE_SECURITY = 12;
    public const int NGHTTP2_HTTP_1_1_REQUIRED = 13;
    public const int NGHTTP2_SESSION_SERVER = 0;
    public const int NGHTTP2_SESSION_CLIENT = 1;
    public const int NGHTTP2_STREAM_STATE_IDLE = 1;
    public const int NGHTTP2_STREAM_STATE_OPEN = 2;
    public const int NGHTTP2_STREAM_STATE_CLOSED = 7;
    public const int NGHTTP2_FLAG_NONE = 0;
    public const int NGHTTP2_FLAG_END_STREAM = 1;
    public const int NGHTTP2_FLAG_END_HEADERS = 4;
    public const int DEFAULT_SETTINGS_HEADER_TABLE_SIZE = 4096;
    public const int DEFAULT_SETTINGS_ENABLE_PUSH = 1;
    public const int DEFAULT_SETTINGS_INITIAL_WINDOW_SIZE = 65535;
    public const int DEFAULT_SETTINGS_MAX_FRAME_SIZE = 16384;
    public const int MIN_MAX_FRAME_SIZE = 16384;
    public const int MAX_MAX_FRAME_SIZE = 16777215;
    public const int MAX_INITIAL_WINDOW_SIZE = 2147483647;

    public static Http2Server createServer(Action<Http2Session>? sessionListener = null)
    {
        return new Http2Server(sessionListener);
    }

    public static Http2SecureServer createSecureServer(Http2SecureServerOptions? options = null, Action<Http2Session>? sessionListener = null)
    {
        _ = options;
        return new Http2SecureServer(sessionListener);
    }

    public static ClientHttp2Session connect(string authority, Action<Http2Session>? listener = null)
    {
        var session = new ClientHttp2Session(authority);
        listener?.Invoke(session);
        return session;
    }

    public static ClientHttp2Session connectSession(string authority, ClientSessionOptions? options = null, Action<Http2Session>? listener = null)
    {
        _ = options;
        return connect(authority, listener);
    }
}

public class Http2Server : EventEmitter
{
    private readonly Http.Server _http1Server;

    public Http2Server(Action<Http2Session>? sessionListener = null)
    {
        _http1Server = new Http.Server((req, res) => res.end());
        if (sessionListener != null)
            on("session", sessionListener);
    }

    public bool listening => _http1Server.listening;

    public Http2Server listen(int port, string? hostname = null, Action? callback = null)
    {
        if (hostname == null)
            _http1Server.listen(port, callback);
        else
            _http1Server.listen(port, hostname, callback);
        return this;
    }

    public Http2Server close(Action? callback = null)
    {
        _http1Server.close(callback);
        return this;
    }

    public Http.AddressInfo? address()
    {
        return _http1Server.address();
    }
}

public sealed class Http2SecureServer : Http2Server
{
    public Http2SecureServer(Action<Http2Session>? sessionListener = null) : base(sessionListener)
    {
    }
}

public sealed class Http2SecureServerOptions : TlsOptions
{
    public bool allowHTTP1 { get; set; }
}

public class Http2Session : EventEmitter
{
    public bool closed { get; private set; }
    public bool destroyed { get; private set; }
    public Http2SessionState state { get; } = new();

    public virtual Http2Stream request(Dictionary<string, string> headers, Action<Http2Stream>? callback = null)
    {
        var stream = new Http2Stream(headers);
        state.streamCount++;
        callback?.Invoke(stream);
        emit("stream", stream, headers);
        return stream;
    }

    public virtual void close(Action? callback = null)
    {
        closed = true;
        emit("close");
        callback?.Invoke();
    }

    public virtual void destroy(Exception? error = null)
    {
        destroyed = true;
        closed = true;
        if (error != null)
            emit("error", error);
        emit("close");
    }
}

public sealed class ClientHttp2Session : Http2Session
{
    public ClientHttp2Session(string authority)
    {
        authority = string.IsNullOrWhiteSpace(authority) ? throw new ArgumentException("Authority is required.", nameof(authority)) : authority;
        this.authority = authority;
    }

    public string authority { get; }
}

public class Http2Stream : EventEmitter
{
    private readonly List<byte> _data = new();

    public Http2Stream(Dictionary<string, string>? headers = null)
    {
        this.headers = headers ?? new Dictionary<string, string>();
    }

    public Dictionary<string, string> headers { get; }
    public bool closed { get; private set; }
    public StreamState state { get; } = new();

    public bool write(string chunk)
    {
        if (closed)
            throw new InvalidOperationException("Cannot write after stream end.");

        _data.AddRange(System.Text.Encoding.UTF8.GetBytes(chunk));
        state.localWindowSize -= chunk.Length;
        return true;
    }

    public void respond(Dictionary<string, string> headers)
    {
        foreach (var header in headers)
            this.headers[header.Key] = header.Value;
    }

    public void priority(StreamPriorityOptions options)
    {
        state.weight = options.weight;
    }

    public void close(int code = 0)
    {
        state.rstCode = code;
        closed = true;
        emit("close");
    }

    public Task end(string? chunk = null)
    {
        if (chunk != null)
            write(chunk);

        closed = true;
        emit("data", _data.ToArray());
        emit("end");
        emit("close");
        return Task.CompletedTask;
    }
}
