using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class dropMembershipTests
{
    [Fact]
    public void dropMembership_LeavesMulticastGroup()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "0.0.0.0");
        Thread.Sleep(100);

        // Add membership first
        socket.addMembership("224.0.0.1");

        // Then drop it - should not throw
        socket.dropMembership("224.0.0.1");

        socket.close();
    }
}
