using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class setMulticastTTLTests
{
    [Fact]
    public void setMulticastTTL_SetsTTL()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var result = socket.setMulticastTTL(128);
        Assert.Equal(128, result);

        socket.close();
    }
}
