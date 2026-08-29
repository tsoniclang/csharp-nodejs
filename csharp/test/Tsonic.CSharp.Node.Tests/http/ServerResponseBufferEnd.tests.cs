using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Tsonic.CSharp.Node;
using Tsonic.CSharp.Node.Http;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class ServerResponseBufferEndTests
{
    [Fact]
    public async Task End_WithBuffer_SendsExactBinaryBody()
    {
        var payload = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x00, 0xFF, 0x7F, 0x01 };

        var server = http.createServer((req, res) =>
        {
            res.statusCode = 200;
            res.setHeader("Content-Type", "application/octet-stream");
            res.end(Buffer.from(payload));
        });

        server.listen(0, "127.0.0.1", (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());

        try
        {
            var port = server.address()!.port;
            using var client = new HttpClient();
            var response = await client.GetAsync($"http://127.0.0.1:{port}/binary");
            var body = await response.Content.ReadAsByteArrayAsync();

            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(payload, body);
        }
        finally
        {
            server.close();
        }
    }

    [Fact]
    public async Task End_WithBuffer_AfterWriteHead_SendsExactBinaryBody()
    {
        var payload = Buffer.from(new[] { 1, 2, 3, 250, 251, 252 });

        var server = http.createServer((req, res) =>
        {
            res.writeHead(200);
            res.end(payload);
        });

        server.listen(0, "127.0.0.1", (Action?)null);
        using var eventLoop = JsEventLoopTestHost.Start(() => server.close());

        try
        {
            var port = server.address()!.port;
            using var client = new HttpClient();
            var response = await client.GetAsync($"http://127.0.0.1:{port}/");
            var body = await response.Content.ReadAsByteArrayAsync();

            Assert.Equal(new byte[] { 1, 2, 3, 250, 251, 252 }, body);
        }
        finally
        {
            server.close();
        }
    }
}
