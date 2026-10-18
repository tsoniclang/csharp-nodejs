using System;
using System.Linq;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class lookupTests
{
    [Fact]
    public void lookup_SimpleDomain_ReturnsAddress()
    {
        string? address = null;
        int family = 0;
        Exception? error = null;

        JsEventLoop.Run(() =>
        {
            dns.lookup("localhost", (err, addr, fam) =>
            {
                error = err;
                address = addr;
                family = fam;
            });
        });

        Assert.Null(error);
        Assert.NotNull(address);
        Assert.True(family == 4 || family == 6);
    }

    [Fact]
    public void lookup_WithIPv4Family_ReturnsIPv4Address()
    {
        string? address = null;
        int family = 0;

        JsEventLoop.Run(() =>
        {
            dns.lookup("localhost", 4, (err, addr, fam) =>
            {
                address = addr;
                family = fam;
            });
        });

        Assert.NotNull(address);
        Assert.Equal(4, family);
    }

    [Fact]
    public void lookup_WithIPv6Family_ReturnsIPv6Address()
    {
        string? address = null;
        int family = 0;

        JsEventLoop.Run(() =>
        {
            dns.lookup("localhost", 6, (err, addr, fam) =>
            {
                address = addr;
                family = fam;
            });
        });

        // May not have IPv6 support on all systems
        Assert.True(family == 0 || family == 6);
    }

    [Fact]
    public void lookup_WithOptionsAll_ReturnsAddressArray()
    {
        JSArray<LookupAddress>? addresses = null;

        JsEventLoop.Run(() =>
        {
            dns.lookup("localhost", new LookupOptions { all = true }, (err, addrs) =>
            {
                addresses = addrs;
            });
        });

        Assert.NotNull(addresses);
        Assert.True(addresses.length > 0);
        Assert.All(addresses, addr =>
        {
            Assert.NotEmpty(addr.address);
            Assert.True(addr.family == 4 || addr.family == 6);
        });
    }

    [Fact]
    public void lookup_WithIPv4FirstOrder_SortsCorrectly()
    {
        JSArray<LookupAddress>? addresses = null;

        JsEventLoop.Run(() =>
        {
            dns.lookup("localhost", new LookupOptions { all = true, order = "ipv4first" }, (err, addrs) =>
            {
                addresses = addrs;
            });
        });

        Assert.NotNull(addresses);

        // Check that IPv4 addresses come before IPv6
        var addressList = addresses.ToList();
        var ipv4Index = addressList.FindIndex(address => address.family == 4);
        var ipv6Index = addressList.FindIndex(address => address.family == 6);

        if (ipv4Index >= 0 && ipv6Index >= 0)
        {
            Assert.True(ipv4Index < ipv6Index);
        }
    }

    [Fact]
    public void lookup_InvalidHostname_ReturnsError()
    {
        Exception? error = null;

        JsEventLoop.Run(() =>
        {
            dns.lookup("this-hostname-definitely-does-not-exist-12345.invalid", (err, addr, fam) =>
            {
                error = err;
            });
        });

        Assert.NotNull(error);
    }

    [Fact]
    public void lookup_WithOptionsFamily_WorksAsExpected()
    {
        int family = 0;

        JsEventLoop.Run(() =>
        {
            dns.lookup("localhost", new LookupOptions { family = 4 }, (err, addr, fam) =>
            {
                family = fam;
            });
        });

        Assert.Equal(4, family);
    }

    [Fact]
    public void lookup_WithStringFamilyIPv4_WorksAsExpected()
    {
        int family = 0;

        JsEventLoop.Run(() =>
        {
            dns.lookup("localhost", new LookupOptions { family = "IPv4" }, (err, addr, fam) =>
            {
                family = fam;
            });
        });

        Assert.Equal(4, family);
    }

    [Fact]
    public void lookup_WithStringFamilyIPv6_WorksAsExpected()
    {
        int family = 0;

        JsEventLoop.Run(() =>
        {
            dns.lookup("localhost", new LookupOptions { family = "IPv6" }, (err, addr, fam) =>
            {
                family = fam;
            });
        });

        // May not have IPv6 support on all systems
        Assert.True(family == 0 || family == 6);
    }
}
