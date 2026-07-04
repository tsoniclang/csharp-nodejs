using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class getSendQueueCountTests
{
    [Fact]
    public void getSendQueueCount_ReturnsZero()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var count = socket.getSendQueueCount();
        Assert.Equal(0, count);

        socket.close();
    }
}
