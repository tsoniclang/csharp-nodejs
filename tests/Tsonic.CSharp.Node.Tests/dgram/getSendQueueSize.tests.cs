using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class getSendQueueSizeTests
{
    [Fact]
    public void getSendQueueSize_ReturnsZero()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var size = socket.getSendQueueSize();
        Assert.Equal(0, size);

        socket.close();
    }
}
