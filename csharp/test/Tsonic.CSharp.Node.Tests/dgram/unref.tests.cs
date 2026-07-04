using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class unrefTests
{
    [Fact]
    public void unref_ReturnsSocket()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var result = socket.unref();
        Assert.Same(socket, result);

        socket.close();
    }
}
