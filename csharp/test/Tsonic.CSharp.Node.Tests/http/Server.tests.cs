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

[Collection(JsEventLoopCollection.Name)]
public partial class HttpServerTests
{


    [Fact]
    public void Server_StreamsBodiesBeyondKestrelDefaultSizeLimit()
    {
        JsEventLoop.Run(async () =>
        {
            const int payloadLength = 30 * 1024 * 1024 + 1;
            long received = 0;
            var server = http.createServer((request, response) =>
            {
                request.onData(chunk => Interlocked.Add(ref received, chunk.length));
                request.onEnd(() => response.end(Interlocked.Read(ref received).ToString()));
            });
            server.listen(0, "127.0.0.1", (Action?)null);
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
        });
    }

    [Fact]
    public void Server_BasicRequest_ReturnsResponse()
    {
        JsEventLoop.Run(async () =>
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
        });
    }

    [Fact]
    public void Server_WaitsForResponseEndAfterListenerReturns()
    {
        JsEventLoop.Run(async () =>
        {
            var server = http.createServer(async (req, res) =>
            {
                await Task.Delay(100);
                res.statusCode = 202;
                res.end("delayed");
            });

            server.listen(0, "127.0.0.1", (Action?)null);

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
        });
    }



    [Fact]
    public void Server_Listen_SetsListeningProperty()
    {
        JsEventLoop.Run(() =>
        {
            // Arrange
            var port = 18083;
            var server = http.createServer((req, res) => res.end("OK"));

            // Assert before
            Assert.False(server.listening);

            // Act
            server.listen(port, (Action?)null);

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
        });
    }

    [Fact]
    public void Server_Address_ReturnsBoundAddressInfo()
    {
        JsEventLoop.Run(() =>
        {
            var port = 18086;
            var server = http.createServer((req, res) => res.end("OK"));

            server.listen(port, "127.0.0.1", (Action?)null);

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
        });
    }

    [Fact]
    public void Server_Address_WithEphemeralPort_ReturnsAssignedPort()
    {
        JsEventLoop.Run(() =>
        {
            var server = http.createServer((req, res) => res.end("OK"));

            server.listen(0, "127.0.0.1", (Action?)null);

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
        });
    }

    [Fact]
    public void Server_Listen_Callback_SeesBoundAddress()
    {
        JsEventLoop.Run(async () =>
        {
            Tsonic.CSharp.Node.Http.ServerAddress? callbackAddress = null;
            var callback = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var server = http.createServer((req, res) => res.end("OK"));

            server.listen(0, "127.0.0.1", null, () =>
            {
                callbackAddress = server.address();
                callback.TrySetResult();
            });

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
        });
    }

    [Fact]
    public void Server_Close_StopsAcceptingConnections()
    {
        JsEventLoop.Run(async () =>
        {
            // Arrange
            var port = 18084;
            var server = http.createServer((req, res) => res.end("OK"));

            server.listen(port, (Action?)null);

            // Act - Close server
            server.close();
            await Task.Delay(500); // Give server time to close

            // Assert - Connection should fail
            using var client = new HttpClient();
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await client.GetAsync($"http://localhost:{port}/");
            });
        });
    }

    [Fact]
    public void ServerResponse_StatusCode_UsesExactIntContract()
    {
        JsEventLoop.Run(async () =>
        {
            var port = 18085;

            var server = http.createServer((req, res) =>
            {
                res.statusCode = 204;
                res.end();
            });

            server.listen(port, (Action?)null);

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
        });
    }

    [Fact]
    public void HttpGet_Response_EmitsDataAndEndEvents()
    {
        JsEventLoop.Run(async () =>
        {
            var port = 18087;
            var chunks = new List<string>();
            var end = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

            var server = http.createServer((req, res) =>
            {
                res.end("payload");
            });

            server.listen(port, (Action?)null);

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
        });
    }

    [Fact]
    public void IncomingMessage_SetTimeout_EmitsTimeoutBeforeCompletion()
    {
        JsEventLoop.Run(async () =>
        {
            var timeout = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var server = http.createServer(async (req, res) =>
            {
                req.setTimeout(20, () => timeout.TrySetResult());
                await Task.Delay(80);
                res.end("OK");
            });

            server.listen(0, "127.0.0.1", (Action?)null);

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
        });
    }

    [Fact]
    public void ServerResponse_SetTimeout_EmitsTimeoutBeforeCompletion()
    {
        JsEventLoop.Run(async () =>
        {
            var timeout = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var server = http.createServer(async (req, res) =>
            {
                res.setTimeout(20, () => timeout.TrySetResult());
                await Task.Delay(80);
                res.end("OK");
            });

            server.listen(0, "127.0.0.1", (Action?)null);

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
        });
    }

    [Fact]
    public void Server_Listen_WithOutOfRangePort_Throws()
    {
        var server = http.createServer((req, res) => res.end("OK"));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            server.listen(70000, (Action?)null));
    }
}
