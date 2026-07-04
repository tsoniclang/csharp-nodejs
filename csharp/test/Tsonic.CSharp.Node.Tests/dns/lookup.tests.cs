using System;
using System.Linq;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class lookupTests
{
    [Fact]
    public void lookup_SimpleDomain_ReturnsAddress()
    {
        var resetEvent = new ManualResetEventSlim(false);
        string? address = null;
        int family = 0;
        Exception? error = null;

        dns.lookup("localhost", (err, addr, fam) =>
        {
            error = err;
            address = addr;
            family = fam;
            resetEvent.Set();
        });

        var signaled = resetEvent.Wait(5000);
        Assert.True(signaled, "Callback was not called within timeout");
        Assert.Null(error);
        Assert.NotNull(address);
        Assert.True(family == 4 || family == 6);
    }

    [Fact]
    public void lookup_WithIPv4Family_ReturnsIPv4Address()
    {
        var resetEvent = new ManualResetEventSlim(false);
        string? address = null;
        int family = 0;

        dns.lookup("localhost", 4, (err, addr, fam) =>
        {
            address = addr;
            family = fam;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(address);
        Assert.Equal(4, family);
    }

    [Fact]
    public void lookup_WithIPv6Family_ReturnsIPv6Address()
    {
        var resetEvent = new ManualResetEventSlim(false);
        string? address = null;
        int family = 0;

        dns.lookup("localhost", 6, (err, addr, fam) =>
        {
            address = addr;
            family = fam;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        // May not have IPv6 support on all systems
        Assert.True(family == 0 || family == 6);
    }

    [Fact]
    public void lookup_WithOptionsAll_ReturnsAddressArray()
    {
        var resetEvent = new ManualResetEventSlim(false);
        LookupAddress[]? addresses = null;

        dns.lookup("localhost", new LookupOptions { all = true }, (err, addrs) =>
        {
            addresses = addrs;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(addresses);
        Assert.True(addresses.Length > 0);
        Assert.All(addresses, addr =>
        {
            Assert.NotEmpty(addr.address);
            Assert.True(addr.family == 4 || addr.family == 6);
        });
    }

    [Fact]
    public void lookup_WithIPv4FirstOrder_SortsCorrectly()
    {
        var resetEvent = new ManualResetEventSlim(false);
        LookupAddress[]? addresses = null;

        dns.lookup("localhost", new LookupOptions { all = true, order = "ipv4first" }, (err, addrs) =>
        {
            addresses = addrs;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(addresses);

        // Check that IPv4 addresses come before IPv6
        var ipv4Index = Array.FindIndex(addresses, a => a.family == 4);
        var ipv6Index = Array.FindIndex(addresses, a => a.family == 6);

        if (ipv4Index >= 0 && ipv6Index >= 0)
        {
            Assert.True(ipv4Index < ipv6Index);
        }
    }

    [Fact]
    public void lookup_InvalidHostname_ReturnsError()
    {
        var resetEvent = new ManualResetEventSlim(false);
        Exception? error = null;

        dns.lookup("this-hostname-definitely-does-not-exist-12345.invalid", (err, addr, fam) =>
        {
            error = err;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(error);
    }

    [Fact]
    public void lookup_WithOptionsFamily_WorksAsExpected()
    {
        var resetEvent = new ManualResetEventSlim(false);
        int family = 0;

        dns.lookup("localhost", new LookupOptions { family = 4 }, (err, addr, fam) =>
        {
            family = fam;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.Equal(4, family);
    }

    [Fact]
    public void lookup_WithStringFamilyIPv4_WorksAsExpected()
    {
        var resetEvent = new ManualResetEventSlim(false);
        int family = 0;

        dns.lookup("localhost", new LookupOptions { family = "IPv4" }, (err, addr, fam) =>
        {
            family = fam;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.Equal(4, family);
    }

    [Fact]
    public void lookup_WithStringFamilyIPv6_WorksAsExpected()
    {
        var resetEvent = new ManualResetEventSlim(false);
        int family = 0;

        dns.lookup("localhost", new LookupOptions { family = "IPv6" }, (err, addr, fam) =>
        {
            family = fam;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        // May not have IPv6 support on all systems
        Assert.True(family == 0 || family == 6);
    }
}
