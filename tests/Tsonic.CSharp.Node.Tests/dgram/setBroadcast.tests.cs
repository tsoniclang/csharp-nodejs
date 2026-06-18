using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class setBroadcastTests
{
    [Fact]
    public void setBroadcast_EnablesBroadcast()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        // Should not throw
        socket.setBroadcast(true);
        socket.setBroadcast(false);

        socket.close();
    }
}
