using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class setTTLTests
{
    [Fact]
    public void setTTL_SetsTTL()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var result = socket.setTTL(128);
        Assert.Equal(128, result);

        socket.close();
    }

    [Fact]
    public void setTTL_InvalidValue_ThrowsException()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        Assert.Throws<ArgumentException>(() => socket.setTTL(0));
        Assert.Throws<ArgumentException>(() => socket.setTTL(256));

        socket.close();
    }
}
