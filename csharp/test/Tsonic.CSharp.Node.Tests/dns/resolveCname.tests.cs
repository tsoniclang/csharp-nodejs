using System;
using System.Threading;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class resolveCnameTests
{
    [Fact]
    public void resolveCname_ValidDomain_ReturnsCnames()
    {
        var resetEvent = new ManualResetEventSlim(false);
        string[]? cnames = null;

        JsEventLoop.Run(() =>
        {
            dns.resolveCname("localhost", (err, names) =>
            {
                cnames = names;
                resetEvent.Set();
            });
        });

        resetEvent.Wait(5000);
        Assert.NotNull(cnames);
    }
}
