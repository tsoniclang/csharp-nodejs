using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class connectTests
{
    [Fact]
    public void connect_ToRemote_ConnectsSuccessfully()
    {
        var server = dgram.createSocket("udp4");
        server.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var addr = server.address();
        var client = dgram.createSocket("udp4");

        var connected = false;
        client.on("connect", (Action)(() =>
        {
            connected = true;
        }));

        client.connect(addr.port, "127.0.0.1");
        Thread.Sleep(100);

        Assert.True(connected);

        var remoteAddr = client.remoteAddress();
        Assert.Equal("127.0.0.1", remoteAddr.address);
        Assert.Equal(addr.port, remoteAddr.port);

        client.close();
        server.close();
    }
}
