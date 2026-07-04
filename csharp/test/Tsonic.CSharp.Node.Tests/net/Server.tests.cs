using System;
using System.Net.Sockets;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class ServerTests
{
    private static int GetListeningPort(Server server)
    {
        var address = server.address();
        Assert.NotNull(address);
        Assert.IsType<AddressInfo>(address);
        return ((AddressInfo)address).port;
    }

    [Fact]
    public void Server_Constructor_CreatesInstance()
    {
        var server = new Server();
        Assert.NotNull(server);
        Assert.False(server.listening);
    }

    [Fact]
    public void Server_ConstructorWithListener_CreatesInstance()
    {
        var server = new Server((socket) => { });
        Assert.NotNull(server);
    }

    [Fact]
    public void Server_ConstructorWithOptions_CreatesInstance()
    {
        var options = new ServerOpts { allowHalfOpen = true };
        var server = new Server(options, null);
        Assert.NotNull(server);
    }

    [Fact]
    public void Server_Listening_InitiallyFalse()
    {
        var server = new Server();
        Assert.False(server.listening);
    }

    [Fact]
    public void Server_MaxConnections_CanBeSet()
    {
        var server = new Server();
        server.maxConnections = 100;
        Assert.Equal(100, server.maxConnections);
    }

    [Fact]
    public void Server_Listen_StartsServer()
    {
        var server = new Server();
        var resetEvent = new ManualResetEventSlim(false);
        var listeningEmitted = false;

        server.on("listening", () =>
        {
            listeningEmitted = true;
            resetEvent.Set();
        });

        server.listen(0, "127.0.0.1");

        resetEvent.Wait(2000);
        Assert.True(listeningEmitted);
        Assert.True(server.listening);

        server.close();
    }

    [Fact]
    public void Server_Listen_WithCallback_CallsCallback()
    {
        var server = new Server();
        var resetEvent = new ManualResetEventSlim(false);
        var callbackCalled = false;

        server.listen(0, "127.0.0.1", () =>
        {
            callbackCalled = true;
            resetEvent.Set();
        });

        resetEvent.Wait(2000);
        Assert.True(callbackCalled);

        server.close();
    }

    [Fact]
    public void Server_Close_StopsServer()
    {
        var server = new Server();
        var listenResetEvent = new ManualResetEventSlim(false);
        var closeResetEvent = new ManualResetEventSlim(false);
        var closeEmitted = false;

        server.on("listening", () => listenResetEvent.Set());
        server.on("close", () =>
        {
            closeEmitted = true;
            closeResetEvent.Set();
        });

        server.listen(0, "127.0.0.1");
        listenResetEvent.Wait(2000);

        server.close();
        closeResetEvent.Wait(2000);

        Assert.True(closeEmitted);
        Assert.False(server.listening);
    }

    [Fact]
    public void Server_Close_WithCallback_CallsCallback()
    {
        var server = new Server();
        var listenResetEvent = new ManualResetEventSlim(false);
        var closeResetEvent = new ManualResetEventSlim(false);
        var callbackCalled = false;

        server.on("listening", () => listenResetEvent.Set());

        server.listen(0, "127.0.0.1");
        listenResetEvent.Wait(2000);

        server.close((err) =>
        {
            callbackCalled = true;
            closeResetEvent.Set();
        });

        closeResetEvent.Wait(2000);
        Assert.True(callbackCalled);
    }

    [Fact]
    public void Server_Address_ReturnsAddressInfo()
    {
        var server = new Server();
        var resetEvent = new ManualResetEventSlim(false);

        server.on("listening", () => resetEvent.Set());
        server.listen(0, "127.0.0.1");
        resetEvent.Wait(2000);

        var address = server.address();
        Assert.NotNull(address);
        Assert.IsType<AddressInfo>(address);

        var addrInfo = (AddressInfo)address;
        Assert.True(addrInfo.port > 0);
        Assert.NotEmpty(addrInfo.address);
        Assert.NotEmpty(addrInfo.family);

        server.close();
    }

    [Fact]
    public void Server_GetConnections_ReturnsCount()
    {
        var server = new Server();
        var resetEvent = new ManualResetEventSlim(false);
        var connectionCount = -1;

        server.on("listening", () => resetEvent.Set());
        server.listen(0, "127.0.0.1");
        resetEvent.Wait(2000);

        server.getConnections((err, count) =>
        {
            connectionCount = count;
        });

        Thread.Sleep(100); // Give callback time to execute
        Assert.Equal(0, connectionCount);

        server.close();
    }

    [Fact]
    public void Server_Unref_ReturnsServer()
    {
        var server = new Server();
        var result = server.unref();
        Assert.Same(server, result);
    }

    [Fact]
    public void Server_Ref_ReturnsServer()
    {
        var server = new Server();
        var result = server.@ref();
        Assert.Same(server, result);
    }

    [Fact]
    public void ServerClient_Connection_EmitsConnectionEvent()
    {
        var server = new Server();
        var serverResetEvent = new ManualResetEventSlim(false);
        var connectionResetEvent = new ManualResetEventSlim(false);
        Socket? serverSocket = null;

        server.on("listening", () => serverResetEvent.Set());
        server.on("connection", (Socket socket) =>
        {
            serverSocket = socket;
            connectionResetEvent.Set();
        });

        server.listen(0, "127.0.0.1");
        serverResetEvent.Wait(2000);

        // Connect a client
        using var client = new TcpClient();
        client.Connect("127.0.0.1", GetListeningPort(server));

        connectionResetEvent.Wait(2000);
        Assert.NotNull(serverSocket);

        server.close();
    }
}
