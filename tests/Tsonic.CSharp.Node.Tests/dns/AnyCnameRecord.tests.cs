using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class AnyCnameRecordTests
{
    [Fact]
    public void AnyCnameRecord_AllProperties_CanBeSet()
    {
        var record = new AnyCnameRecord
        {
            value = "www.example.com"
        };

        Assert.Equal("CNAME", record.type);
        Assert.Equal("www.example.com", record.value);
    }
}
