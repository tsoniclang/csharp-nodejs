using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class AnyNsRecordTests
{
    [Fact]
    public void AnyNsRecord_AllProperties_CanBeSet()
    {
        var record = new AnyNsRecord
        {
            value = "ns1.example.com"
        };

        Assert.Equal("NS", record.type);
        Assert.Equal("ns1.example.com", record.value);
    }
}
