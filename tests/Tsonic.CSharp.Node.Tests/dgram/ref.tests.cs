using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class refTests
{
    [Fact]
    public void ref_ReturnsSocket()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var result = socket.@ref();
        Assert.Same(socket, result);

        socket.close();
    }
}
