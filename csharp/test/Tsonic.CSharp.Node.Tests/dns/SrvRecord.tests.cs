using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class SrvRecordTests
{
    [Fact]
    public void SrvRecord_AllProperties_CanBeSet()
    {
        var record = new SrvRecord
        {
            priority = 10,
            weight = 5,
            port = 21223,
            name = "service.example.com"
        };

        Assert.Equal(10, record.priority);
        Assert.Equal(5, record.weight);
        Assert.Equal(21223, record.port);
        Assert.Equal("service.example.com", record.name);
    }
}
