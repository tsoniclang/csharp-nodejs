using System;
using System.Threading;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class resolveMxTests
{
    [Fact]
    public void resolveMx_ValidDomain_ReturnsMxRecords()
    {
        var resetEvent = new ManualResetEventSlim(false);
        MxRecord[]? records = null;

        JsEventLoop.Run(() =>
        {
            dns.resolveMx("localhost", (err, recs) =>
            {
                records = recs;
                resetEvent.Set();
            });
        });

        resetEvent.Wait(5000);
        Assert.NotNull(records);
    }
}
