using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Tsonic.CSharp.Node.Http;

namespace Tsonic.CSharp.Node.Tests;

public class HttpServerTests
{
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

            res.writeHead(200, new System.Collections.Generic.Dictionary<string, string>
            {
                { "Content-Type", "text/plain" }
            });

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
    }

    [Fact]
    public async Task Server_CustomHeaders_ReturnsCorrectHeaders()
    {
        // Arrange
        var port = 18081;

        var server = http.createServer((req, res) =>
        {
            res.writeHead(200, new System.Collections.Generic.Dictionary<string, string>
            {
                { "Content-Type", "application/json" },
                { "X-Custom-Header", "test-value" }
            });

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
    }

    [Fact]
    public async Task Server_RequestHeaders_AreAccessible()
    {
        // Arrange
        var port = 18082;
        string? receivedUserAgent = null;

        var server = http.createServer((req, res) =>
        {
            receivedUserAgent = req.headers.GetValueOrDefault("user-agent");
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

        try
        {
            var address = server.address();

            Assert.NotNull(address);
            Assert.Equal(port, address.port);
            Assert.Equal("127.0.0.1", address.address);
            Assert.Equal("IPv4", address.family);
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

        try
        {
            var address = server.address();

            Assert.NotNull(address);
            Assert.True(address.port > 0);
            Assert.Equal("127.0.0.1", address.address);
            Assert.Equal("IPv4", address.family);
        }
        finally
        {
            server.close();
        }
    }

    [Fact]
    public void Server_Listen_Callback_SeesBoundAddress()
    {
        Tsonic.CSharp.Node.Http.AddressInfo? callbackAddress = null;
        var callback = false;
        var server = http.createServer((req, res) => res.end("OK"));

        server.listen(0, "127.0.0.1", null, () =>
        {
            callback = true;
            callbackAddress = server.address();
        });

        try
        {
            Assert.True(callback);
            Assert.NotNull(callbackAddress);
            Assert.True(callbackAddress.port > 0);
            Assert.Equal("127.0.0.1", callbackAddress.address);
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

        try
        {
            http.get($"http://127.0.0.1:{port}/", (res) =>
            {
                res.on("data", (string chunk) =>
                {
                    chunks.Add(chunk);
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
        var server = http.createServer((req, res) =>
        {
            req.setTimeout(20, () => timeout.TrySetResult());
            Thread.Sleep(80);
            res.end("OK");
        });

        server.listen(0, "127.0.0.1", (Action?)null);

        try
        {
            var address = server.address();
            Assert.NotNull(address);
            using var client = new HttpClient();
            var responseTask = client.GetAsync($"http://127.0.0.1:{address.port}/");

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
        var server = http.createServer((req, res) =>
        {
            res.setTimeout(20, () => timeout.TrySetResult());
            Thread.Sleep(80);
            res.end("OK");
        });

        server.listen(0, "127.0.0.1", (Action?)null);

        try
        {
            var address = server.address();
            Assert.NotNull(address);
            using var client = new HttpClient();
            var responseTask = client.GetAsync($"http://127.0.0.1:{address.port}/");

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
