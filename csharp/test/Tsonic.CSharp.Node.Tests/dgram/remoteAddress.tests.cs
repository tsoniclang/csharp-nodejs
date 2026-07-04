using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class remoteAddressTests
{
    [Fact]
    public void remoteAddress_UnconnectedSocket_ThrowsException()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        Assert.Throws<InvalidOperationException>(() => socket.remoteAddress());
        socket.close();
    }
}
