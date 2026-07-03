using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class setDefaultResultOrderTests
{
    [Fact]
    public void setDefaultResultOrder_IPv4First_UpdatesOrder()
    {
        dns.setDefaultResultOrder("ipv4first");
        var order = dns.getDefaultResultOrder();
        Assert.Equal("ipv4first", order);

        // Reset to default
        dns.setDefaultResultOrder("verbatim");
    }

    [Fact]
    public void setDefaultResultOrder_IPv6First_UpdatesOrder()
    {
        dns.setDefaultResultOrder("ipv6first");
        var order = dns.getDefaultResultOrder();
        Assert.Equal("ipv6first", order);

        // Reset to default
        dns.setDefaultResultOrder("verbatim");
    }

    [Fact]
    public void setDefaultResultOrder_Verbatim_UpdatesOrder()
    {
        dns.setDefaultResultOrder("verbatim");
        var order = dns.getDefaultResultOrder();
        Assert.Equal("verbatim", order);
    }

    [Fact]
    public void setDefaultResultOrder_InvalidValue_ThrowsError()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            dns.setDefaultResultOrder("invalid");
        });
    }
}
