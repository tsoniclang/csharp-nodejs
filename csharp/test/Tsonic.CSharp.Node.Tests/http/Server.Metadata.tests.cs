using Tsonic.CSharp.Js;
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

public partial class HttpServerTests
{
    [Fact]
    public void Server_RetainedRequestMetadataSurvivesTheNextKeepAliveRequest()
    {
        JsEventLoop.Run(async () =>
        {
            IncomingMessage? first = null;
            var connections = 0;
            var server = http.createServer((request, response) =>
            {
                first ??= request;
                response.end("ok");
            });
            server.listen(0, "127.0.0.1", (Action?)null);
            try
            {
                using var handler = new SocketsHttpHandler
                {
                    MaxConnectionsPerServer = 1,
                    ConnectCallback = async (context, cancellationToken) =>
                    {
                        Interlocked.Increment(ref connections);
                        var socket = new System.Net.Sockets.Socket(SocketType.Stream, ProtocolType.Tcp);
                        try
                        {
                            await socket.ConnectAsync(context.DnsEndPoint, cancellationToken);
                            return new System.Net.Sockets.NetworkStream(socket, ownsSocket: true);
                        }
                        catch
                        {
                            socket.Dispose();
                            throw;
                        }
                    },
                };
                using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) };
                var address = $"http://127.0.0.1:{server.address()!.address!.port}";
                using var initial = new HttpRequestMessage(HttpMethod.Post, $"{address}/first?one=1");
                initial.Headers.Add("x-request", "first");
                using var initialReply = await client.SendAsync(initial);
                Assert.Equal("ok", await initialReply.Content.ReadAsStringAsync());
                using var next = new HttpRequestMessage(HttpMethod.Get, $"{address}/second");
                next.Headers.Add("x-request", "second");
                using var nextReply = await client.SendAsync(next);
                Assert.Equal("ok", await nextReply.Content.ReadAsStringAsync());
                Assert.Equal(1, connections);
                Assert.NotNull(first);
                Assert.Equal("POST", first.method);
                Assert.Equal("/first?one=1", first.url);
                Assert.Equal("first", first.headers.get("x-request"));
                Assert.Equal("1.1", first.httpVersion);
            }
            finally
            {
                server.close();
            }
        });
    }

    [Fact]
    public void Server_HeadersDistinctPreservesRepeatedValuesAndSnapshotIsolation()
    {
        JsEventLoop.Run(async () =>
        {
            string[]? first = null;
            string[]? second = null;
            string[]? missing = null;
            var sameCarrier = false;
            var server = http.createServer((request, response) =>
            {
                sameCarrier = ReferenceEquals(request.headers, request.headersDistinct);
                first = request.headersDistinct.getAll("x-item");
                first![0] = "changed";
                second = request.headersDistinct.getAll("X-ITEM");
                missing = request.headersDistinct["missing"] is null ? null : throw new InvalidOperationException();
                response.end("ok");
            });
            server.listen(0, "127.0.0.1", (Action?)null);
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
        });
    }

    [Fact]
    public void Server_CustomHeaders_ReturnsCorrectHeaders()
    {
        JsEventLoop.Run(async () =>
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
        });
    }

    [Fact]
    public void Server_RequestHeaders_AreAccessible()
    {
        JsEventLoop.Run(async () =>
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
        });
    }

}
