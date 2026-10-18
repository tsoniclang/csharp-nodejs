using System.Threading.Tasks;
using Tsonic.CSharp.Js;
using System;
using System.Net.Sockets;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
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
        JsEventLoop.Run(async () =>
        {
            var server = new Server();
            var resetEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var listeningEmitted = false;

            server.on("listening", () =>
            {
                listeningEmitted = true;
                resetEvent.TrySetResult(true);
            });

            server.listen(0, "127.0.0.1");
            try
            {

                await resetEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(2000));
                Assert.True(listeningEmitted);
                Assert.True(server.listening);

                server.close();
            }
            finally
            {
                server.close();
            }
        });
    }

    [Fact]
    public void Server_Listen_WithCallback_CallsCallback()
    {
        JsEventLoop.Run(async () =>
        {
            var server = new Server();
            var resetEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var callbackCalled = false;

            server.listen(0, "127.0.0.1", () =>
            {
                callbackCalled = true;
                resetEvent.TrySetResult(true);
            });
            try
            {

                await resetEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(2000));
                Assert.True(callbackCalled);

                server.close();
            }
            finally
            {
                server.close();
            }
        });
    }

    [Fact]
    public void Server_Close_StopsServer()
    {
        JsEventLoop.Run(async () =>
        {
            var server = new Server();
            var listenResetEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var closeResetEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var closeEmitted = false;

            server.on("listening", () => listenResetEvent.TrySetResult(true));
            server.on("close", () =>
            {
                closeEmitted = true;
                closeResetEvent.TrySetResult(true);
            });

            server.listen(0, "127.0.0.1");
            try
            {
                await listenResetEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(2000));

                server.close();
                await closeResetEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(2000));

                Assert.True(closeEmitted);
                Assert.False(server.listening);
            }
            finally
            {
                server.close();
            }
        });
    }

    [Fact]
    public void Server_Close_WithCallback_CallsCallback()
    {
        JsEventLoop.Run(async () =>
        {
            var server = new Server();
            var listenResetEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var closeResetEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var callbackCalled = false;

            server.on("listening", () => listenResetEvent.TrySetResult(true));

            server.listen(0, "127.0.0.1");
            try
            {
                await listenResetEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(2000));

                server.close((err) =>
                {
                    callbackCalled = true;
                    closeResetEvent.TrySetResult(true);
                });

                await closeResetEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(2000));
                Assert.True(callbackCalled);
            }
            finally
            {
                server.close();
            }
        });
    }

    [Fact]
    public void Server_Address_ReturnsAddressInfo()
    {
        JsEventLoop.Run(async () =>
        {
            var server = new Server();
            var resetEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            server.on("listening", () => resetEvent.TrySetResult(true));
            server.listen(0, "127.0.0.1");
            try
            {
                await resetEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(2000));

                var address = server.address();
                Assert.NotNull(address);
                Assert.IsType<AddressInfo>(address);

                var addrInfo = (AddressInfo)address;
                Assert.True(addrInfo.port > 0);
                Assert.NotEmpty(addrInfo.address);
                Assert.NotEmpty(addrInfo.family);

                server.close();
            }
            finally
            {
                server.close();
            }
        });
    }

    [Fact]
    public void Server_GetConnections_ReturnsCount()
    {
        JsEventLoop.Run(async () =>
        {
            var server = new Server();
            var resetEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var connectionsRead = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var connectionCount = -1;

            server.on("listening", () => resetEvent.TrySetResult(true));
            server.listen(0, "127.0.0.1");
            try
            {
                await resetEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(2000));

                server.getConnections((err, count) =>
                {
                    connectionCount = count;
                    connectionsRead.TrySetResult(true);
                });

                Assert.True(await connectionsRead.Task.WaitAsync(TimeSpan.FromMilliseconds(2000)));
                Assert.Equal(0, connectionCount);

                server.close();
            }
            finally
            {
                server.close();
            }
        });
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
        JsEventLoop.Run(async () =>
        {
            var server = new Server();
            var serverResetEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var connectionResetEvent = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Socket? serverSocket = null;

            server.on("listening", () => serverResetEvent.TrySetResult(true));
            server.on("connection", (Socket socket) =>
            {
                serverSocket = socket;
                connectionResetEvent.TrySetResult(true);
            });

            server.listen(0, "127.0.0.1");
            try
            {
                await serverResetEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(2000));

                // Connect a client
                using var client = new TcpClient();
                client.Connect("127.0.0.1", GetListeningPort(server));

                await connectionResetEvent.Task.WaitAsync(TimeSpan.FromMilliseconds(2000));
                Assert.NotNull(serverSocket);

                server.close();
            }
            finally
            {
                serverSocket?.destroy();
                server.close();
            }
        });
    }
}
