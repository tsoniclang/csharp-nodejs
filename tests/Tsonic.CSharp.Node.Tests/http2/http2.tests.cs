using System.Collections.Generic;
using Tsonic.CSharp.Node.Http2;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class Http2Tests
{
    [Fact]
    public void Connect_CreatesClientSession()
    {
        var session = http2.connect("https://example.com");

        Assert.Equal("https://example.com", session.authority);
        Assert.False(session.closed);
        Assert.False(session.destroyed);
    }

    [Fact]
    public async System.Threading.Tasks.Task Stream_RecordsHeadersAndCloses()
    {
        var session = http2.connect("https://example.com");
        var stream = session.request(new Dictionary<string, string> { ["path"] = "/" });

        stream.respond(new Dictionary<string, string> { ["status"] = "200" });
        stream.write("ok");
        await stream.end();

        Assert.Equal("200", stream.headers["status"]);
        Assert.True(stream.closed);
    }

    [Fact]
    public void Session_Close_EmitsCloseAndMarksClosed()
    {
        var session = http2.connect("https://example.com");
        var closed = false;
        session.on("close", () => closed = true);

        session.close();

        Assert.True(session.closed);
        Assert.True(closed);
    }

    [Fact]
    public void Session_DestroyWithError_EmitsErrorAndClose()
    {
        var session = http2.connect("https://example.com");
        var errorSeen = false;
        var closeSeen = false;
        session.on("error", (object? value) => errorSeen = value is System.Exception);
        session.on("close", () => closeSeen = true);

        session.destroy(new System.InvalidOperationException("boom"));

        Assert.True(session.destroyed);
        Assert.True(session.closed);
        Assert.True(errorSeen);
        Assert.True(closeSeen);
    }

    [Fact]
    public void Connect_RejectsEmptyAuthority()
    {
        Assert.Throws<System.ArgumentException>(() => http2.connect(""));
    }

    [Fact]
    public void Constants_ExposeCoreHttp2Names()
    {
        Assert.True(http2.constants.ContainsKey("NGHTTP2_SESSION_SERVER"));
        Assert.True(http2.constants.ContainsKey("HTTP2_HEADER_STATUS"));
    }
}
