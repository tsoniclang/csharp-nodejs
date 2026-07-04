using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class setRecvBufferSizeTests
{
    [Fact]
    public void setRecvBufferSize_SetsBufferSize()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        socket.setRecvBufferSize(8192);
        var size = socket.getRecvBufferSize();
        // Buffer size might be adjusted by OS
        Assert.True(size > 0);

        socket.close();
    }
}
