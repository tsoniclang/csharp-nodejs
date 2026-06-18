using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class AnyTxtRecordTests
{
    [Fact]
    public void AnyTxtRecord_AllProperties_CanBeSet()
    {
        var record = new AnyTxtRecord
        {
            entries = new[] { "v=spf1 include:_spf.example.com ~all" }
        };

        Assert.Equal("TXT", record.type);
        Assert.Single(record.entries);
        Assert.Equal("v=spf1 include:_spf.example.com ~all", record.entries[0]);
    }
}
