using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Tsonic.CSharp.Node.Http;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class HttpServerTests
{
    [Fact]
    public async Task Server_HeadersDistinctPreservesRepeatedValuesAndSnapshotIsolation()
    {
        string[]? first = null;
        string[]? second = null;
        string[]? missing = null;
        var sameCarrier = false;
        var server = http.createServer((request, response) =>
        {
            sameCarrier = ReferenceEquals(request.headers, request.headersDistinct);
            first = request.headersDistinct["x-item"];
            first![0] = "changed";
            second = request.headersDistinct["X-ITEM"];
            missing = request.headersDistinct["missing"];
            response.end("ok");
        });
        server.listen(0, "127.0.0.1", (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());
        try
        {
            using var client = new TcpClient();
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await client.ConnectAsync("127.0.0.1", server.address()!.address!.port, deadline.Token);
            var stream = client.GetStream();
            await stream.WriteAsync(Encoding.ASCII.GetBytes(
                "GET / HTTP/1.1\r\nHost: localhost\r\nX-Item: one\r\nX-Item: two\r\nConnection: close\r\n\r\n"), deadline.Token);
            var reply = new byte[256];
            var read = await stream.ReadAsync(reply, deadline.Token);
            Assert.True(read > 0);
            Assert.True(sameCarrier);
            Assert.Equal(new[] { "changed", "two" }, first);
            Assert.Equal(new[] { "one", "two" }, second);
            Assert.Null(missing);
        }
        finally
        {
            server.close();
        }
    }

    [Fact]
    public async Task Server_StreamsBodiesBeyondKestrelDefaultSizeLimit()
    {
        const int payloadLength = 30 * 1024 * 1024 + 1;
        long received = 0;
        var server = http.createServer((request, response) =>
        {
            request.onData(chunk => Interlocked.Add(ref received, chunk.length));
            request.onEnd(() => response.end(Interlocked.Read(ref received).ToString()));
        });
        server.listen(0, "127.0.0.1", (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            using var content = new ByteArrayContent(new byte[payloadLength]);
            using var reply = await client.PostAsync(
                $"http://127.0.0.1:{server.address()!.address!.port}/upload", content);
            Assert.Equal(System.Net.HttpStatusCode.OK, reply.StatusCode);
            Assert.Equal(payloadLength.ToString(), await reply.Content.ReadAsStringAsync());
        }
        finally
        {
            server.close();
        }
    }

    [Fact]
    public async Task Server_BasicRequest_ReturnsResponse()
    {
        // Arrange
        var port = 18080;
        var receivedRequest = false;
        string? receivedMethod = null;
        string? receivedUrl = null;

        var server = http.createServer((req, res) =>
        {
            receivedRequest = true;
            receivedMethod = req.method;
            receivedUrl = req.url;

            res.setHeader("Content-Type", "text/plain");
            res.writeHead(200, res.getHeaders());

            res.end("Hello World");
        });

        server.listen(port, (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());

        try
        {
            // Act - Make HTTP request to server
            using var client = new HttpClient();
            var response = await client.GetAsync($"http://localhost:{port}/test");
            var body = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(receivedRequest);
            Assert.Equal("GET", receivedMethod);
            Assert.Equal("/test", receivedUrl);
            Assert.Equal("Hello World", body);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            // Cleanup
            server.close();
            await Task.Delay(500); // Give server time to close
        }
    }

    [Fact]
    public async Task Server_WaitsForResponseEndAfterListenerReturns()
    {
        var server = http.createServer(async (req, res) =>
        {
            await Task.Delay(100);
            res.statusCode = 202;
            res.end("delayed");
        });

        server.listen(0, "127.0.0.1", (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());

        try
        {
            var address = server.address();
            Assert.NotNull(address);
            using var client = new HttpClient();
            var response = await client.GetAsync($"http://127.0.0.1:{address.address!.port}/");
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(System.Net.HttpStatusCode.Accepted, response.StatusCode);
            Assert.Equal("delayed", body);
        }
        finally
        {
            server.close();
        }
    }

    [Fact]
    public async Task Server_CustomHeaders_ReturnsCorrectHeaders()
    {
        // Arrange
        var port = 18081;

        var server = http.createServer((req, res) =>
        {
            res.setHeader("Content-Type", "application/json");
            res.setHeader("X-Custom-Header", "test-value");
            res.writeHead(200, res.getHeaders());

            res.end("{\"status\":\"ok\"}");
        });

        server.listen(port, (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());

        try
        {
            // Act
            using var client = new HttpClient();
            var response = await client.GetAsync($"http://localhost:{port}/");

            // Assert
            Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
            Assert.True(response.Headers.Contains("X-Custom-Header"));
            Assert.Equal("test-value", response.Headers.GetValues("X-Custom-Header").First());
        }
        finally
        {
            // Cleanup
            server.close();
            await Task.Delay(500);
        }
    }

    [Fact]
    public async Task Server_RequestHeaders_AreAccessible()
    {
        // Arrange
        var port = 18082;
        string? receivedUserAgent = null;

        var server = http.createServer((req, res) =>
        {
            receivedUserAgent = req.headers.get("user-agent");
            res.end("OK");
        });

        server.listen(port, (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());

        try
        {
            // Act
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "TestAgent/1.0");
            await client.GetAsync($"http://localhost:{port}/");

            // Assert
            Assert.NotNull(receivedUserAgent);
            Assert.Contains("TestAgent/1.0", receivedUserAgent);
        }
        finally
        {
            // Cleanup
            server.close();
            await Task.Delay(500);
        }
    }

    [Fact]
    public void Server_Listen_SetsListeningProperty()
    {
        // Arrange
        var port = 18083;
        var server = http.createServer((req, res) => res.end("OK"));

        // Assert before
        Assert.False(server.listening);

        // Act
        server.listen(port, (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());

        try
        {
            // Assert after
            Assert.True(server.listening);
        }
        finally
        {
            // Cleanup
            server.close();
        }
    }

    [Fact]
    public void Server_Address_ReturnsBoundAddressInfo()
    {
        var port = 18086;
        var server = http.createServer((req, res) => res.end("OK"));

        server.listen(port, "127.0.0.1", (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());

        try
        {
            var address = server.address();

            Assert.NotNull(address);
            Assert.Equal(port, address.address!.port);
            Assert.Equal("127.0.0.1", address.address.address);
            Assert.Equal("IPv4", address.address.family);
        }
        finally
        {
            server.close();
        }
    }

    [Fact]
    public void Server_Address_WithEphemeralPort_ReturnsAssignedPort()
    {
        var server = http.createServer((req, res) => res.end("OK"));

        server.listen(0, "127.0.0.1", (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());

        try
        {
            var address = server.address();

            Assert.NotNull(address);
            Assert.True(address.address!.port > 0);
            Assert.Equal("127.0.0.1", address.address.address);
            Assert.Equal("IPv4", address.address.family);
        }
        finally
        {
            server.close();
        }
    }

    [Fact]
    public async Task Server_Listen_Callback_SeesBoundAddress()
    {
        Tsonic.CSharp.Node.Http.ServerAddress? callbackAddress = null;
        var callback = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var server = http.createServer((req, res) => res.end("OK"));

        server.listen(0, "127.0.0.1", null, () =>
        {
            callbackAddress = server.address();
            callback.TrySetResult();
        });
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());

        try
        {
            await callback.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.NotNull(callbackAddress);
            Assert.True(callbackAddress.address!.port > 0);
            Assert.Equal("127.0.0.1", callbackAddress.address.address);
        }
        finally
        {
            server.close();
        }
    }

    [Fact]
    public async Task Server_Close_StopsAcceptingConnections()
    {
        // Arrange
        var port = 18084;
        var server = http.createServer((req, res) => res.end("OK"));

        server.listen(port, (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());

        // Act - Close server
        server.close();
        await Task.Delay(500); // Give server time to close

        // Assert - Connection should fail
        using var client = new HttpClient();
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await client.GetAsync($"http://localhost:{port}/");
        });
    }

    [Fact]
    public async Task ServerResponse_StatusCode_UsesExactIntContract()
    {
        var port = 18085;

        var server = http.createServer((req, res) =>
        {
            res.statusCode = 204;
            res.end();
        });

        server.listen(port, (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());

        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync($"http://localhost:{port}/");

            Assert.Equal(System.Net.HttpStatusCode.NoContent, response.StatusCode);
        }
        finally
        {
            server.close();
            await Task.Delay(500);
        }
    }

    [Fact]
    public async Task HttpGet_Response_EmitsDataAndEndEvents()
    {
        var port = 18087;
        var chunks = new List<string>();
        var end = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        var server = http.createServer((req, res) =>
        {
            res.end("payload");
        });

        server.listen(port, (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());

        try
        {
            http.get($"http://127.0.0.1:{port}/", (res) =>
            {
                res.on("data", (Buffer chunk) =>
                {
                    chunks.Add(chunk.toString());
                });
                res.on("end", () =>
                {
                    end.TrySetResult();
                });
            });

            await end.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(new[] { "payload" }, chunks);
        }
        finally
        {
            server.close();
        }
    }

    [Fact]
    public async Task IncomingMessage_SetTimeout_EmitsTimeoutBeforeCompletion()
    {
        var timeout = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var server = http.createServer(async (req, res) =>
        {
            req.setTimeout(20, () => timeout.TrySetResult());
            await Task.Delay(80);
            res.end("OK");
        });

        server.listen(0, "127.0.0.1", (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());

        try
        {
            var address = server.address();
            Assert.NotNull(address);
            using var client = new HttpClient();
            var responseTask = client.GetAsync($"http://127.0.0.1:{address.address!.port}/");

            await timeout.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var response = await responseTask;
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            server.close();
        }
    }

    [Fact]
    public async Task ServerResponse_SetTimeout_EmitsTimeoutBeforeCompletion()
    {
        var timeout = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var server = http.createServer(async (req, res) =>
        {
            res.setTimeout(20, () => timeout.TrySetResult());
            await Task.Delay(80);
            res.end("OK");
        });

        server.listen(0, "127.0.0.1", (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());

        try
        {
            var address = server.address();
            Assert.NotNull(address);
            using var client = new HttpClient();
            var responseTask = client.GetAsync($"http://127.0.0.1:{address.address!.port}/");

            await timeout.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var response = await responseTask;
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            server.close();
        }
    }

    [Fact]
    public void Server_Listen_WithOutOfRangePort_Throws()
    {
        var server = http.createServer((req, res) => res.end("OK"));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            server.listen(70000, (Action?)null));
    }
}
