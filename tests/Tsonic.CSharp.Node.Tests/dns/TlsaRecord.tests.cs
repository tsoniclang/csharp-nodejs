using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class TlsaRecordTests
{
    [Fact]
    public void TlsaRecord_AllProperties_CanBeSet()
    {
        var record = new TlsaRecord
        {
            certUsage = 3,
            selector = 1,
            match = 1,
            data = new byte[] { 1, 2, 3, 4 }
        };

        Assert.Equal(3, record.certUsage);
        Assert.Equal(1, record.selector);
        Assert.Equal(1, record.match);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, record.data);
    }
}
