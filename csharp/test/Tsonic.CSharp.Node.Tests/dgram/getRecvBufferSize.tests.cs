using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class getRecvBufferSizeTests
{
    [Fact]
    public void getRecvBufferSize_ReturnsBufferSize()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var size = socket.getRecvBufferSize();
        Assert.True(size > 0);

        socket.close();
    }
}
