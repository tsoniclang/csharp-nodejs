using System.Threading.Tasks;
using System;
using System.Threading;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public partial class SocketTests
{
    [Fact]
    public void Socket_Connect_StartsConnection()
    {
        JsEventLoop.Run(async () =>
        {
            var socket = new Socket();
            Socket? acceptedSocket = null;
            var server = net.createServer(client => acceptedSocket = client);
            var listenEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var connectEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var observedConnecting = false;

            server.listen(0, "127.0.0.1", () =>
            {
                socket.connect(GetListeningPort(server), "127.0.0.1", () =>
                {
                    connectEvent.TrySetResult(true);
                });
                observedConnecting = socket.connecting || socket.readyState == "opening" || socket.readyState == "open";
                listenEvent.TrySetResult(true);
            });
            try
            {

                Assert.True(await listenEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(2000)), "Server should start listening");
                Assert.True(observedConnecting, "Socket should begin opening or already be open");
                Assert.True(await connectEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(5000)), "Connect callback should be invoked");
            }
            finally
            {
                socket.destroy();
                acceptedSocket?.destroy();
                server.close();
            }
        });
    }

    [Fact]
    public void Socket_DataEvent_EmitsReceivedData()
    {
        JsEventLoop.Run(async () =>
        {
            var dataReceived = false;
            var receivedMessage = "";
            var resetEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var testMessage = "Hello, Socket!";
            Socket? client = null;
            Socket? acceptedSocket = null;

            // Create a server
            var server = net.createServer((Socket clientSocket) =>
            {
                acceptedSocket = clientSocket;
                clientSocket.on("data", (Buffer data) =>
                {
                    dataReceived = true;
                    receivedMessage = data.toString();
                    resetEvent.TrySetResult(true);
                });
            });

            server.listen(0, () =>
            {
                client = new Socket();
                client.connect(GetListeningPort(server), "localhost", () =>
                {
                    client.write(testMessage);
                });
            });
            try
            {

                var signaled = await resetEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(5000));

                Assert.True(signaled, "Data event should have been received within timeout");
                Assert.True(dataReceived, "Data event should have been emitted");
                Assert.Equal(testMessage, receivedMessage);
            }
            finally
            {
                client?.destroy();
                acceptedSocket?.destroy();
                server.close();
            }
        });
    }

    [Fact]
    public void Socket_DataEvent_EmitsMultipleChunks()
    {
        JsEventLoop.Run(async () =>
        {
            const string expected = "Chunk1Chunk2Chunk3";
            var combined = "";
            var resetEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Socket? client = null;
            Socket? acceptedSocket = null;

            // Create a server
            var server = net.createServer((Socket clientSocket) =>
            {
                acceptedSocket = clientSocket;
                clientSocket.on("data", (Buffer data) =>
                {
                    combined += data.toString();
                    if (combined.Length >= expected.Length)
                    {
                        resetEvent.TrySetResult(true);
                    }
                });
            });

            server.listen(0, () =>
            {
                client = new Socket();
                client.connect(GetListeningPort(server), "localhost", () =>
                {
                    client.write("Chunk1");
                    client.write("Chunk2");
                    client.write("Chunk3");
                });
            });
            try
            {

                var signaled = await resetEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(5000));

                Assert.True(signaled, "Should have received all chunk data within timeout");
                Assert.Equal(expected, combined);
            }
            finally
            {
                client?.destroy();
                acceptedSocket?.destroy();
                server.close();
            }
        });
    }

    [Fact]
    public void Socket_EndEvent_EmitsOnConnectionClose()
    {
        JsEventLoop.Run(async () =>
        {
            var endReceived = false;
            var resetEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Socket? client = null;
            Socket? acceptedSocket = null;

            // Create a server
            var server = net.createServer((Socket clientSocket) =>
            {
                acceptedSocket = clientSocket;
                clientSocket.on("end", () =>
                {
                    endReceived = true;
                    resetEvent.TrySetResult(true);
                });
            });

            server.listen(0, () =>
            {
                client = new Socket();
                client.connect(GetListeningPort(server), "localhost", () =>
                {
                    client.end();
                });
            });
            try
            {

                var signaled = await resetEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(5000));

                Assert.True(signaled, "End event should have been received within timeout");
                Assert.True(endReceived, "End event should have been emitted");
            }
            finally
            {
                client?.destroy();
                acceptedSocket?.destroy();
                server.close();
            }
        });
    }

    [Fact]
    public void Socket_WriteAndReceive_RoundTrip()
    {
        JsEventLoop.Run(async () =>
        {
            var serverReceived = "";
            var clientReceived = "";
            var resetEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Socket? client = null;
            Socket? acceptedSocket = null;

            // Create a server that echoes data
            var server = net.createServer((Socket clientSocket) =>
            {
                acceptedSocket = clientSocket;
                clientSocket.on("data", (Buffer data) =>
                {
                    serverReceived = data.toString();
                    // Echo back with a prefix
                    clientSocket.write("Echo: " + serverReceived);
                });
            });

            server.listen(0, () =>
            {
                client = new Socket();
                client.on("data", (Buffer data) =>
                {
                    clientReceived = data.toString();
                    resetEvent.TrySetResult(true);
                });

                client.connect(GetListeningPort(server), "localhost", () =>
                {
                    client.write("Test message");
                });
            });
            try
            {

                var signaled = await resetEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(5000));

                Assert.True(signaled, "Should have completed round-trip within timeout");
                Assert.Equal("Test message", serverReceived);
                Assert.Equal("Echo: Test message", clientReceived);
            }
            finally
            {
                client?.destroy();
                acceptedSocket?.destroy();
                server.close();
            }
        });
    }

}
