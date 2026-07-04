using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class resolveTlsaTests
{
    [Fact]
    public void resolveTlsa_ValidDomain_ReturnsTlsaRecords()
    {
        var resetEvent = new ManualResetEventSlim(false);
        TlsaRecord[]? records = null;

        dns.resolveTlsa("localhost", (err, recs) =>
        {
            records = recs;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(records);
    }
}
