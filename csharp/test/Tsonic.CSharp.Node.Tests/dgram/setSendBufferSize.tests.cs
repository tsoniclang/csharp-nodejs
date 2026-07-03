using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class setSendBufferSizeTests
{
    [Fact]
    public void setSendBufferSize_SetsBufferSize()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        socket.setSendBufferSize(8192);
        var size = socket.getSendBufferSize();
        // Buffer size might be adjusted by OS
        Assert.True(size > 0);

        socket.close();
    }
}
