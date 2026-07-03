using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class getServersTests
{
    [Fact]
    public void getServers_ReturnsServerArray()
    {
        dns.setServers(new[] { "1.1.1.1" });

        var servers = dns.getServers();

        Assert.NotNull(servers);
        Assert.IsType<string[]>(servers);
        Assert.Equal(new[] { "1.1.1.1" }, servers);
    }

    [Fact]
    public void getServers_ReturnsCopy()
    {
        dns.setServers(new[] { "9.9.9.9" });

        var servers = dns.getServers();
        servers[0] = "8.8.8.8";

        Assert.Equal(new[] { "9.9.9.9" }, dns.getServers());
    }
}
