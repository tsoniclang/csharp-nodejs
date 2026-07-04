using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class resolveSoaTests
{
    [Fact]
    public void resolveSoa_ValidDomain_ReturnsSoaRecord()
    {
        var resetEvent = new ManualResetEventSlim(false);
        SoaRecord? record = null;

        dns.resolveSoa("localhost", (err, rec) =>
        {
            record = rec;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(record);
    }
}
