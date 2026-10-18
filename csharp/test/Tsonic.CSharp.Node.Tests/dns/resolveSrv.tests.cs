using System;
using System.Threading;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class resolveSrvTests
{
    [Fact]
    public void resolveSrv_ValidDomain_ReturnsSrvRecords()
    {
        var resetEvent = new ManualResetEventSlim(false);
        SrvRecord[]? records = null;

        JsEventLoop.Run(() =>
        {
            dns.resolveSrv("localhost", (err, recs) =>
            {
                records = recs;
                resetEvent.Set();
            });
        });

        resetEvent.Wait(5000);
        Assert.NotNull(records);
    }
}
