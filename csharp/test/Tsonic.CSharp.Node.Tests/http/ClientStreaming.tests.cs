using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Tsonic.CSharp.Node.Http;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public sealed class ClientStreamingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ResponseIsAvailableBeforeBodyAndRetainsExactBytes(bool abort)
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var releaseBody = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var responseReady = new TaskCompletionSource<IncomingMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var bodyReady = new TaskCompletionSource<Task<Buffer>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var server = Task.Run(async () =>
        {
            using var connection = await listener.AcceptTcpClientAsync();
            await using var stream = connection.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);
            while (!string.IsNullOrEmpty(await reader.ReadLineAsync())) { }
            await stream.WriteAsync("HTTP/1.1 200 OK\r\nContent-Length: 4\r\nConnection: close\r\n\r\n"u8.ToArray());
            await releaseBody.Task.WaitAsync(TimeSpan.FromSeconds(15));
            if (!abort) await stream.WriteAsync(new byte[] { 0, 255, 128, 65 });
        });
        var request = http.request($"http://127.0.0.1:{port}/", response =>
        {
            response.onClose(() => closed.TrySetResult());
            bodyReady.SetResult(response.readAllBuffer());
            responseReady.SetResult(response);
        });
        var sent = request.end();
        using var eventLoop = JsEventLoopTestHost.Start(request.abort);
        try
        {
            var response = await responseReady.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var body = await bodyReady.Task;
            Assert.Equal(200, response.statusCode);
            Assert.False(body.IsCompleted);
            await Assert.ThrowsAsync<InvalidOperationException>(response.readAll);
            if (abort)
            {
                request.abort();
                var error = await Assert.ThrowsAnyAsync<Exception>(() => body.WaitAsync(TimeSpan.FromSeconds(10)));
                Assert.IsNotType<TimeoutException>(error);
                Assert.False(response.complete);
            }
            else
            {
                releaseBody.SetResult();
                var bytes = await body.WaitAsync(TimeSpan.FromSeconds(10));
                Assert.Equal(4, bytes.length);
                Assert.Equal(255, bytes[1]);
                Assert.Equal(128, bytes[2]);
                Assert.True(response.complete);
            }
            await closed.Task.WaitAsync(TimeSpan.FromSeconds(10));
            await sent;
        }
        finally
        {
            releaseBody.TrySetResult();
            request.abort();
            await server.WaitAsync(TimeSpan.FromSeconds(10));
        }
    }
}
