using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class RecordWithTtlTests
{
    [Fact]
    public void RecordWithTtl_AllProperties_CanBeSet()
    {
        var record = new RecordWithTtl
        {
            address = "127.0.0.1",
            ttl = 300
        };

        Assert.Equal("127.0.0.1", record.address);
        Assert.Equal(300, record.ttl);
    }
}
