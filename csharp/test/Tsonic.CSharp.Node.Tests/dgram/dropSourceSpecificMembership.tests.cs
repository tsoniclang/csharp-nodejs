using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class dropSourceSpecificMembershipTests
{
    [Fact]
    public void dropSourceSpecificMembership_ThrowsNotSupported()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "0.0.0.0");
        Thread.Sleep(100);

        Assert.Throws<NotSupportedException>(() => socket.dropSourceSpecificMembership("192.168.1.1", "224.0.0.1"));

        socket.close();
    }
}
