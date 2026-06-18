using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class NaptrRecordTests
{
    [Fact]
    public void NaptrRecord_AllProperties_CanBeSet()
    {
        var record = new NaptrRecord
        {
            flags = "s",
            service = "SIP+D2U",
            regexp = "",
            replacement = "_sip._udp.example.com",
            order = 30,
            preference = 100
        };

        Assert.Equal("s", record.flags);
        Assert.Equal("SIP+D2U", record.service);
        Assert.Equal("", record.regexp);
        Assert.Equal("_sip._udp.example.com", record.replacement);
        Assert.Equal(30, record.order);
        Assert.Equal(100, record.preference);
    }
}
