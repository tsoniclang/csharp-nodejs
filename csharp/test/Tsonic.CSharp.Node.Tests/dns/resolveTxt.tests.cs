using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class resolveTxtTests
{
    [Fact]
    public void resolveTxt_ValidDomain_ReturnsTxtRecords()
    {
        var resetEvent = new ManualResetEventSlim(false);
        string[][]? records = null;

        dns.resolveTxt("localhost", (err, recs) =>
        {
            records = recs;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(records);
    }
}
