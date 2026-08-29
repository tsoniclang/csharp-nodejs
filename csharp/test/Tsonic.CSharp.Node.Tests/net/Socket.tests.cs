using System;
using System.Threading;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class SocketTests
{
    private const int TEST_PORT = 18234;

    private static int GetListeningPort(Server server)
    {
        var address = server.address();
        Assert.NotNull(address);
        Assert.IsType<AddressInfo>(address);
        return ((AddressInfo)address).port;
    }

    [Fact]
    public void Socket_Constructor_CreatesInstance()
    {
        var socket = new Socket();
        Assert.NotNull(socket);
        Assert.Equal("closed", socket.readyState);
    }

    [Fact]
    public void Socket_ConstructorWithOptions_CreatesInstance()
    {
        var options = new SocketConstructorOpts { allowHalfOpen = true };
        var socket = new Socket(options);
        Assert.NotNull(socket);
    }

    [Fact]
    public void Socket_BytesRead_InitiallyZero()
    {
        var socket = new Socket();
        Assert.Equal(0, socket.bytesRead);
    }

    [Fact]
    public void Socket_BytesWritten_InitiallyZero()
    {
        var socket = new Socket();
        Assert.Equal(0, socket.bytesWritten);
    }

    [Fact]
    public void Socket_Connecting_InitiallyFalse()
    {
        var socket = new Socket();
        Assert.False(socket.connecting);
    }

    [Fact]
    public void Socket_Destroyed_InitiallyFalse()
    {
        var socket = new Socket();
        Assert.False(socket.destroyed);
    }

    [Fact]
    public void Socket_ReadyState_InitiallyClosed()
    {
        var socket = new Socket();
        Assert.Equal("closed", socket.readyState);
    }

    [Fact]
    public void Socket_Connect_StartsConnection()
    {
        var socket = new Socket();
        Socket? acceptedSocket = null;
        var server = net.createServer(client => acceptedSocket = client);
        var listenEvent = new ManualResetEventSlim(false);
        var connectEvent = new ManualResetEventSlim(false);
        var observedConnecting = false;

        server.listen(0, "127.0.0.1", () =>
        {
            socket.connect(GetListeningPort(server), "127.0.0.1", () =>
            {
                connectEvent.Set();
            });
            observedConnecting = socket.connecting || socket.readyState == "opening" || socket.readyState == "open";
            listenEvent.Set();
        });
        using var eventLoop = JsEventLoopTestHost.Start(() =>
        {
            socket.destroy();
            acceptedSocket?.destroy();
            server.close();
        });

        Assert.True(listenEvent.Wait(2000), "Server should start listening");
        Assert.True(observedConnecting, "Socket should begin opening or already be open");
        Assert.True(connectEvent.Wait(5000), "Connect callback should be invoked");

    }

    [Fact]
    public void Socket_Destroy_MarksAsDestroyed()
    {
        var socket = new Socket();
        socket.destroy();
        JsEventLoop.Run();
        Assert.True(socket.destroyed);
        Assert.Equal("closed", socket.readyState);
    }

    [Fact]
    public void Socket_Destroy_EmitsCloseEvent()
    {
        var socket = new Socket();
        var closeEmitted = false;

        socket.on("close", (bool hadError) =>
        {
            closeEmitted = true;
        });

        socket.destroy();
        JsEventLoop.Run();

        Assert.True(closeEmitted);
    }

    [Fact]
    public void Socket_DestroyWithError_EmitsError()
    {
        var socket = new Socket();
        var errorEmitted = false;

        socket.on("error", (Exception err) =>
        {
            errorEmitted = true;
        });

        socket.destroy(new Exception("Test error"));
        JsEventLoop.Run();

        Assert.True(errorEmitted);
    }

    [Fact]
    public void Socket_SetTimeout_DoesNotThrow()
    {
        var socket = new Socket();
        var exception = Record.Exception(() =>
        {
            socket.setTimeout(5000);
        });
        Assert.Null(exception);
    }

    [Fact]
    public void Socket_SetNoDelay_DoesNotThrow()
    {
        var socket = new Socket();
        var exception = Record.Exception(() =>
        {
            socket.setNoDelay(true);
        });
        Assert.Null(exception);
    }

    [Fact]
    public void Socket_SetKeepAlive_DoesNotThrow()
    {
        var socket = new Socket();
        var exception = Record.Exception(() =>
        {
            socket.setKeepAlive(true, 1000);
        });
        Assert.Null(exception);
    }

    [Fact]
    public void Socket_Address_ReturnsEmptyObjectWhenNotConnected()
    {
        var socket = new Socket();
        var address = socket.address();
        Assert.NotNull(address);
    }

    [Fact]
    public void Socket_Unref_ReturnsSocket()
    {
        var socket = new Socket();
        var result = socket.unref();
        Assert.Same(socket, result);
    }

    [Fact]
    public void Socket_Ref_ReturnsSocket()
    {
        var socket = new Socket();
        var result = socket.@ref();
        Assert.Same(socket, result);
    }

    [Fact]
    public void Socket_Pause_ReturnsSocket()
    {
        var socket = new Socket();
        var result = socket.pause();
        Assert.Same(socket, result);
    }

    [Fact]
    public void Socket_Resume_ReturnsSocket()
    {
        var socket = new Socket();
        var result = socket.resume();
        Assert.Same(socket, result);
    }

    [Fact]
    public void Socket_SetEncoding_ReturnsSocket()
    {
        var socket = new Socket();
        var result = socket.setEncoding("utf8");
        Assert.Same(socket, result);
    }

    [Fact]
    public void Socket_End_ReturnsSocket()
    {
        var socket = new Socket();
        var result = socket.end();
        Assert.Same(socket, result);
    }

    [Fact]
    public void Socket_ResetAndDestroy_ReturnsSocket()
    {
        var socket = new Socket();
        var result = socket.resetAndDestroy();
        JsEventLoop.Run();
        Assert.Same(socket, result);
        Assert.True(socket.destroyed);
    }

    [Fact]
    public void Socket_DataEvent_EmitsReceivedData()
    {
        var dataReceived = false;
        var receivedMessage = "";
        var resetEvent = new ManualResetEventSlim(false);
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
                resetEvent.Set();
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
        using var eventLoop = JsEventLoopTestHost.Start(() =>
        {
            client?.destroy();
            acceptedSocket?.destroy();
            server.close();
        });

        var signaled = resetEvent.Wait(5000);

        Assert.True(signaled, "Data event should have been received within timeout");
        Assert.True(dataReceived, "Data event should have been emitted");
        Assert.Equal(testMessage, receivedMessage);
    }

    [Fact]
    public void Socket_DataEvent_EmitsMultipleChunks()
    {
        const string expected = "Chunk1Chunk2Chunk3";
        var combined = "";
        var resetEvent = new ManualResetEventSlim(false);
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
                    resetEvent.Set();
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
        using var eventLoop = JsEventLoopTestHost.Start(() =>
        {
            client?.destroy();
            acceptedSocket?.destroy();
            server.close();
        });

        var signaled = resetEvent.Wait(5000);

        Assert.True(signaled, "Should have received all chunk data within timeout");
        Assert.Equal(expected, combined);
    }

    [Fact]
    public void Socket_EndEvent_EmitsOnConnectionClose()
    {
        var endReceived = false;
        var resetEvent = new ManualResetEventSlim(false);
        Socket? client = null;
        Socket? acceptedSocket = null;

        // Create a server
        var server = net.createServer((Socket clientSocket) =>
        {
            acceptedSocket = clientSocket;
            clientSocket.on("end", () =>
            {
                endReceived = true;
                resetEvent.Set();
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
        using var eventLoop = JsEventLoopTestHost.Start(() =>
        {
            client?.destroy();
            acceptedSocket?.destroy();
            server.close();
        });

        var signaled = resetEvent.Wait(5000);

        Assert.True(signaled, "End event should have been received within timeout");
        Assert.True(endReceived, "End event should have been emitted");
    }

    [Fact]
    public void Socket_WriteAndReceive_RoundTrip()
    {
        var serverReceived = "";
        var clientReceived = "";
        var resetEvent = new ManualResetEventSlim(false);
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
                resetEvent.Set();
            });

            client.connect(GetListeningPort(server), "localhost", () =>
            {
                client.write("Test message");
            });
        });
        using var eventLoop = JsEventLoopTestHost.Start(() =>
        {
            client?.destroy();
            acceptedSocket?.destroy();
            server.close();
        });

        var signaled = resetEvent.Wait(5000);

        Assert.True(signaled, "Should have completed round-trip within timeout");
        Assert.Equal("Test message", serverReceived);
        Assert.Equal("Echo: Test message", clientReceived);
    }
}
