using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class setMulticastLoopbackTests
{
    [Fact]
    public void setMulticastLoopback_SetsLoopback()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var result = socket.setMulticastLoopback(true);
        Assert.True(result);

        socket.close();
    }
}
