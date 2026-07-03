using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class resolveNaptrTests
{
    [Fact]
    public void resolveNaptr_ValidDomain_ReturnsNaptrRecords()
    {
        var resetEvent = new ManualResetEventSlim(false);
        NaptrRecord[]? records = null;

        dns.resolveNaptr("localhost", (err, recs) =>
        {
            records = recs;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(records);
    }
}
