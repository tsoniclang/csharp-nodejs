using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class disconnectTests
{
    [Fact]
    public void disconnect_ConnectedSocket_Disconnects()
    {
        var server = dgram.createSocket("udp4");
        server.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var addr = server.address();
        var client = dgram.createSocket("udp4");
        client.connect(addr.port, "127.0.0.1");
        Thread.Sleep(100);

        // Should not throw
        client.disconnect();

        // Should throw after disconnect
        Assert.Throws<InvalidOperationException>(() => client.remoteAddress());

        client.close();
        server.close();
    }
}
