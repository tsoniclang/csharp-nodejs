using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class getSendBufferSizeTests
{
    [Fact]
    public void getSendBufferSize_ReturnsBufferSize()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var size = socket.getSendBufferSize();
        Assert.True(size > 0);

        socket.close();
    }
}
