using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class AnyPtrRecordTests
{
    [Fact]
    public void AnyPtrRecord_AllProperties_CanBeSet()
    {
        var record = new AnyPtrRecord
        {
            value = "example.com"
        };

        Assert.Equal("PTR", record.type);
        Assert.Equal("example.com", record.value);
    }
}
