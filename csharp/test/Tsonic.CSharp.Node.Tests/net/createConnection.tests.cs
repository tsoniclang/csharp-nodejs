using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class createConnectionTests
{
    [Fact]
    public void createConnection_CreatesSocket()
    {
        Socket? acceptedSocket = null;
        Socket? socket = null;
        var connected = new ManualResetEventSlim();
        var server = net.createServer(client => acceptedSocket = client);
        server.listen(0, "127.0.0.1");
        var address = Assert.IsType<AddressInfo>(server.address());
        using var eventLoop = JsEventLoopTestHost.Start(() =>
        {
            socket?.destroy();
            acceptedSocket?.destroy();
            server.close();
        });

        socket = net.createConnection(address.port, "127.0.0.1", connected.Set);

        Assert.True(connected.Wait(TimeSpan.FromSeconds(5)));
        Assert.NotNull(socket);
        Assert.IsType<Socket>(socket);
    }
}
