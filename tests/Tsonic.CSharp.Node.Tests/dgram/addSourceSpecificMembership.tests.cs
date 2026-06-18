using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class addSourceSpecificMembershipTests
{
    [Fact]
    public void addSourceSpecificMembership_ThrowsNotSupported()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "0.0.0.0");
        Thread.Sleep(100);

        Assert.Throws<NotSupportedException>(() => socket.addSourceSpecificMembership("192.168.1.1", "224.0.0.1"));

        socket.close();
    }
}
