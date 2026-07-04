using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class closeTests
{
    [Fact]
    public void close_BoundSocket_ClosesSuccessfully()
    {
        var socket = dgram.createSocket("udp4");
        var closed = false;

        socket.on("close", (Action)(() =>
        {
            closed = true;
        }));

        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        socket.close();
        Thread.Sleep(100);

        Assert.True(closed);
    }
}
