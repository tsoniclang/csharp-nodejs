using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class MxRecordTests
{
    [Fact]
    public void MxRecord_AllProperties_CanBeSet()
    {
        var record = new MxRecord
        {
            priority = 10,
            exchange = "mail.example.com"
        };

        Assert.Equal(10, record.priority);
        Assert.Equal("mail.example.com", record.exchange);
    }
}
