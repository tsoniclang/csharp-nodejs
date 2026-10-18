using System;
using System.Threading;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class resolve6Tests
{
    [Fact]
    public void resolve6_ValidDomain_ReturnsIPv6Addresses()
    {
        var resetEvent = new ManualResetEventSlim(false);
        string[]? addresses = null;

        JsEventLoop.Run(() =>
        {
            dns.resolve6("localhost", (err, addrs) =>
            {
                addresses = addrs;
                resetEvent.Set();
            });
        });

        resetEvent.Wait(5000);
        Assert.NotNull(addresses);
    }

    [Fact]
    public void resolve6_WithTtlOption_ReturnsRecordsWithTtl()
    {
        var resetEvent = new ManualResetEventSlim(false);
        object? result = null;

        JsEventLoop.Run(() =>
        {
            dns.resolve6("localhost", new ResolveOptions { ttl = true }, (err, res) =>
            {
                result = res;
                resetEvent.Set();
            });
        });

        resetEvent.Wait(5000);
        Assert.NotNull(result);
        Assert.IsType<RecordWithTtl[]>(result);
    }
}
