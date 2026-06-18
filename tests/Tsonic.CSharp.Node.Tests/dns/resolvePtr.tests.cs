using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class resolvePtrTests
{
    [Fact]
    public void resolvePtr_ValidDomain_ReturnsPtrRecords()
    {
        var resetEvent = new ManualResetEventSlim(false);
        string[]? records = null;

        dns.resolvePtr("localhost", (err, recs) =>
        {
            records = recs;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(records);
    }
}
