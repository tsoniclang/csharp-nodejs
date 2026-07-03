using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class setMulticastInterfaceTests
{
    [Fact]
    public void setMulticastInterface_SetsInterface()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "0.0.0.0");
        Thread.Sleep(100);

        // Should not throw when setting a valid local interface IP
        socket.setMulticastInterface("127.0.0.1");

        socket.close();
    }
}
