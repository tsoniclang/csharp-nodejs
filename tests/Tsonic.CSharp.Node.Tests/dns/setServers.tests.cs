using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class setServersTests
{
    [Fact]
    public void setServers_ValidServers_DoesNotThrow()
    {
        var servers = new[] { "8.8.8.8", "8.8.4.4" };

        var exception = Record.Exception(() => dns.setServers(servers));

        Assert.Null(exception);
        Assert.Equal(servers, dns.getServers());
    }
}
