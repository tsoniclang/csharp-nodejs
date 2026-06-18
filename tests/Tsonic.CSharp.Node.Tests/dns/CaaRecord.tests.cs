using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class CaaRecordTests
{
    [Fact]
    public void CaaRecord_AllProperties_CanBeSet()
    {
        var record = new CaaRecord
        {
            critical = 0,
            issue = "example.com",
            issuewild = "*.example.com",
            iodef = "mailto:security@example.com",
            contactemail = "security@example.com",
            contactphone = "+1-555-0100"
        };

        Assert.Equal(0, record.critical);
        Assert.Equal("example.com", record.issue);
        Assert.Equal("*.example.com", record.issuewild);
        Assert.Equal("mailto:security@example.com", record.iodef);
        Assert.Equal("security@example.com", record.contactemail);
        Assert.Equal("+1-555-0100", record.contactphone);
    }
}
