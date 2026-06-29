#pragma warning disable CS1591
#pragma warning disable IDE1006

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node.Http2;

public sealed class Http2Settings
{
    public int? headerTableSize { get; set; }
    public bool? enablePush { get; set; }
    public int? initialWindowSize { get; set; }
    public int? maxFrameSize { get; set; }
    public int? maxConcurrentStreams { get; set; }
    public int? maxHeaderListSize { get; set; }
}

public class SessionOptions
{
    public int? maxDeflateDynamicTableSize { get; set; }
    public int? maxHeaderListPairs { get; set; }
    public int? maxOutstandingPings { get; set; }
    public int? maxSendHeaderBlockLength { get; set; }
    public int? maxSessionMemory { get; set; }
    public int? maxSettings { get; set; }
    public int? paddingStrategy { get; set; }
    public int? peerMaxConcurrentStreams { get; set; }
    public int[]? remoteCustomSettings { get; set; }
    public Http2Settings? settings { get; set; }
    public bool strictFieldWhitespaceValidation { get; set; } = true;
    public int? unknownProtocolTimeout { get; set; }
}

public sealed class ClientSessionOptions : SessionOptions
{
    public bool priorKnowledge { get; set; }
    public string? protocol { get; set; }
    public int? maxReservedRemoteStreams { get; set; }
    public object? session { get; set; }
}

public sealed class ServerOptions : SessionOptions
{
    public bool allowHTTP1 { get; set; }
}

public sealed class ClientSessionRequestOptions
{
    public bool endStream { get; set; }
    public bool exclusive { get; set; }
    public int? parent { get; set; }
    public int? weight { get; set; }
}

public sealed class AlternativeServiceOptions
{
    public string origin { get; set; } = string.Empty;
}

public class ServerStreamResponseOptions
{
    public bool endStream { get; set; }
    public bool waitForTrailers { get; set; }
}

public class ServerStreamFileResponseOptions : ServerStreamResponseOptions
{
    public long? offset { get; set; }
    public long? length { get; set; }
}

public sealed class ServerStreamFileResponseOptionsWithError : ServerStreamFileResponseOptions
{
    public Action<Exception?>? onError { get; set; }
}

public sealed class StatOptions
{
    public bool bigint { get; set; }
}

public sealed class StreamPriorityOptions
{
    public bool exclusive { get; set; }
    public int? parent { get; set; }
    public int weight { get; set; } = 16;
    public bool silent { get; set; }
}

public sealed class Http2SessionState
{
    public int effectiveLocalWindowSize { get; set; } = 65535;
    public int effectiveRecvDataLength { get; set; }
    public int nextStreamID { get; set; } = 1;
    public int localWindowSize { get; set; } = 65535;
    public int lastProcStreamID { get; set; }
    public int remoteWindowSize { get; set; } = 65535;
    public int outboundQueueSize { get; set; }
    public int deflateDynamicTableSize { get; set; }
    public int inflateDynamicTableSize { get; set; }
    public int streamCount { get; set; }
}

public sealed class StreamState
{
    public int state { get; set; } = http2.NGHTTP2_STREAM_STATE_OPEN;
    public int weight { get; set; } = 16;
    public int sumDependencyWeight { get; set; }
    public int localClose { get; set; }
    public int remoteClose { get; set; }
    public int localWindowSize { get; set; } = 65535;
    public int? rstCode { get; set; }
}

public sealed class Http2ServerRequest : Readable
{
    public Http2ServerRequest(Dictionary<string, string>? headers = null)
    {
        this.headers = headers ?? [];
        method = this.headers.TryGetValue(http2.HTTP2_HEADER_METHOD, out var methodValue) ? methodValue : http2.HTTP2_METHOD_GET;
        url = this.headers.TryGetValue(http2.HTTP2_HEADER_PATH, out var pathValue) ? pathValue : "/";
    }

    public Dictionary<string, string> headers { get; }
    public string method { get; }
    public string url { get; }

    public Http2ServerRequest setTimeout(int milliseconds, Action? callback = null)
    {
        _ = milliseconds;
        callback?.Invoke();
        return this;
    }
}

public sealed class Http2ServerResponse : Writable
{
    private readonly Dictionary<string, object?> _headers = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<object?> _trailers = [];
    private readonly List<object?> _body = [];

    public object?[] body => _body.ToArray();

    public void setHeader(string name, object? value) => _headers[name] = value;

    public void appendHeader(string name, object? value)
    {
        if (_headers.TryGetValue(name, out var existing))
            _headers[name] = new[] { existing, value };
        else
            _headers[name] = value;
    }

    public object? getHeader(string name) => _headers.TryGetValue(name, out var value) ? value : null;

    public Http2ServerResponse writeHead(int statusCode, Dictionary<string, object?>? headers = null)
    {
        setHeader(http2.HTTP2_HEADER_STATUS, statusCode);
        if (headers != null)
            foreach (var header in headers)
                setHeader(header.Key, header.Value);
        return this;
    }

    public void writeContinue() => setHeader("continue", true);

    public void writeEarlyHints(Dictionary<string, object?> hints) => setHeader("early-hints", hints);

    public void addTrailers(Dictionary<string, object?> trailers)
    {
        _trailers.Add(trailers);
    }

    public new bool write(object? chunk, string? encoding = null, Action? callback = null)
    {
        _body.Add(chunk);
        return base.write(chunk, encoding, callback);
    }

    public Http2ServerResponse setTimeout(int milliseconds, Action? callback = null)
    {
        _ = milliseconds;
        callback?.Invoke();
        return this;
    }
}
