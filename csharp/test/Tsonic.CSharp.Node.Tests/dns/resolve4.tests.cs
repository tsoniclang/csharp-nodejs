using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class resolve4Tests
{
    [Fact]
    public void resolve4_ValidDomain_ReturnsIPv4Addresses()
    {
        var resetEvent = new ManualResetEventSlim(false);
        string[]? addresses = null;

        dns.resolve4("localhost", (err, addrs) =>
        {
            addresses = addrs;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(addresses);
    }

    [Fact]
    public void resolve4_WithTtlOption_ReturnsRecordsWithTtl()
    {
        var resetEvent = new ManualResetEventSlim(false);
        object? result = null;

        dns.resolve4("localhost", new ResolveOptions { ttl = true }, (err, res) =>
        {
            result = res;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(result);
        Assert.IsType<RecordWithTtl[]>(result);
    }

    [Fact]
    public void resolve4_WithoutTtlOption_ReturnsStringArray()
    {
        var resetEvent = new ManualResetEventSlim(false);
        object? result = null;

        dns.resolve4("localhost", new ResolveOptions { ttl = false }, (err, res) =>
        {
            result = res;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(result);
        Assert.IsType<string[]>(result);
    }
}
