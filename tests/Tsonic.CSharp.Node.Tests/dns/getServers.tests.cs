using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class getServersTests
{
    [Fact]
    public void getServers_ReturnsServerArray()
    {
        var servers = dns.getServers();
        Assert.NotNull(servers);
        Assert.IsType<string[]>(servers);
    }
}
