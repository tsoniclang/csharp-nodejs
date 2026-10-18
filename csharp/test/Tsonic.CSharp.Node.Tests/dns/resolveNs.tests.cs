using System;
using System.Threading;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class resolveNsTests
{
    [Fact]
    public void resolveNs_ValidDomain_ReturnsNameServers()
    {
        var resetEvent = new ManualResetEventSlim(false);
        string[]? nameservers = null;

        JsEventLoop.Run(() =>
        {
            dns.resolveNs("localhost", (err, ns) =>
            {
                nameservers = ns;
                resetEvent.Set();
            });
        });

        resetEvent.Wait(5000);
        Assert.NotNull(nameservers);
    }
}
