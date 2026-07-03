using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class addMembershipTests
{
    [Fact]
    public void addMembership_JoinsMulticastGroup()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "0.0.0.0");
        Thread.Sleep(100);

        // Should not throw
        socket.addMembership("224.0.0.1");
        socket.dropMembership("224.0.0.1");

        socket.close();
    }
}
