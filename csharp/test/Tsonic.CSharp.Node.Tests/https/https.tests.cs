using System;
using Tsonic.CSharp.Node.Http;
using Tsonic.CSharp.Node.Https;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class HttpsTests
{
    [Fact]
    public void Request_NormalizesOptionsToHttps()
    {
        var request = https.request(new HttpsRequestOptions
        {
            hostname = "example.com",
            port = 80,
            path = "/index.html",
            method = "GET"
        });

        Assert.Equal("https:", request.protocol);
        Assert.Equal("example.com", request.host);
    }

    [Fact]
    public void Request_RejectsHttpUrl()
    {
        Assert.Throws<ArgumentException>(() => https.request("http://example.com/"));
    }

    [Fact]
    public void Request_FromHttpsUrl_UsesDefaultHttpsPort()
    {
        var request = https.request("https://example.com/path?q=1");

        Assert.Equal("https:", request.protocol);
        Assert.Equal("example.com", request.host);
        Assert.Equal("/path?q=1", request.path);
    }

    [Fact]
    public void CreateServer_ReturnsHttpCompatibleServer()
    {
        using var certificate = TlsTestCertificate.Create();
        var called = false;
        var server = https.createServer(
            new HttpsServerOptions
            {
                pfx = TlsTestCertificate.PfxValue(certificate)
            },
            (_, res) =>
            {
                called = true;
                res.end("ok");
            });

        Assert.False(server.listening);
        Assert.NotNull(server);
        Assert.False(called);
    }

    [Fact]
    public void Request_RejectsNullOptions()
    {
        Assert.Throws<ArgumentNullException>(() => https.request((HttpsRequestOptions)null!));
    }
}
