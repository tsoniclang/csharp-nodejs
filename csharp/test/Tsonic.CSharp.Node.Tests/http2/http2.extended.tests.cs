using System.Collections.Generic;
using Tsonic.CSharp.Node.Http2;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class Http2ExtendedTests
{
    [Fact]
    public void Constants_ShouldExposeHeadersMethodsStatusesAndNghttp2Codes()
    {
        Assert.Equal(":method", http2.HTTP2_HEADER_METHOD);
        Assert.Equal("GET", http2.HTTP2_METHOD_GET);
        Assert.Equal(200, http2.HTTP_STATUS_OK);
        Assert.Equal(8, http2.NGHTTP2_CANCEL);
        Assert.Equal(16384, http2.DEFAULT_SETTINGS_MAX_FRAME_SIZE);
    }

    [Fact]
    public void ConnectSession_ShouldAcceptOptionsAndTrackState()
    {
        var session = http2.connectSession("https://example.com", new ClientSessionOptions
        {
            protocol = "https:",
            priorKnowledge = false,
            settings = new Http2Settings { enablePush = false }
        });

        var stream = session.request(new Dictionary<string, string> { [http2.HTTP2_HEADER_PATH] = "/" });

        Assert.Equal("https://example.com", session.authority);
        Assert.Equal(1, session.state.streamCount);
        Assert.Equal("/", stream.headers[http2.HTTP2_HEADER_PATH]);
    }

    [Fact]
    public void StreamPriorityAndClose_ShouldUpdateState()
    {
        var stream = new Http2Stream();

        stream.priority(new StreamPriorityOptions { weight = 32 });
        stream.close(http2.NGHTTP2_CANCEL);

        Assert.Equal(32, stream.state.weight);
        Assert.Equal(http2.NGHTTP2_CANCEL, stream.state.rstCode);
        Assert.True(stream.closed);
    }

    [Fact]
    public void ServerRequest_ShouldExposeMethodPathAndTimeout()
    {
        var request = new Http2ServerRequest(new Dictionary<string, string>
        {
            [http2.HTTP2_HEADER_METHOD] = http2.HTTP2_METHOD_POST,
            [http2.HTTP2_HEADER_PATH] = "/submit"
        });
        var called = false;

        request.setTimeout(1, () => called = true);

        Assert.Equal(http2.HTTP2_METHOD_POST, request.method);
        Assert.Equal("/submit", request.url);
        Assert.True(called);
    }

    [Fact]
    public void ServerResponse_ShouldManageHeadersBodyAndTimeout()
    {
        var response = new Http2ServerResponse();
        var called = false;

        response.setHeader("x-a", "1");
        response.appendHeader("x-a", "2");
        response.writeHead(201, new Dictionary<string, object?> { ["content-type"] = "text/plain" });
        response.writeContinue();
        response.writeEarlyHints(new Dictionary<string, object?> { ["link"] = "</a.css>" });
        response.addTrailers(new Dictionary<string, object?> { ["x-t"] = "t" });
        response.write("body");
        response.setTimeout(1, () => called = true);

        Assert.Equal(201, response.getHeader(http2.HTTP2_HEADER_STATUS));
        Assert.Equal("text/plain", response.getHeader("content-type"));
        Assert.Equal(["body"], response.body);
        Assert.True(called);
    }
}
