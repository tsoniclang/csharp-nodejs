using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class resolveAnyTests
{
    [Fact]
    public void resolveAny_ValidDomain_ReturnsAnyRecords()
    {
        var resetEvent = new ManualResetEventSlim(false);
        object[]? records = null;

        dns.resolveAny("localhost", (err, recs) =>
        {
            records = recs;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(records);
    }
}
