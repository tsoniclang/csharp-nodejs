using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class resolveCaaTests
{
    [Fact]
    public void resolveCaa_ValidDomain_ReturnsCaaRecords()
    {
        var resetEvent = new ManualResetEventSlim(false);
        CaaRecord[]? records = null;

        dns.resolveCaa("localhost", (err, recs) =>
        {
            records = recs;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(records);
    }
}
