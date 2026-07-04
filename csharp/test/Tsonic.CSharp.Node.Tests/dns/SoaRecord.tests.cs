using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class SoaRecordTests
{
    [Fact]
    public void SoaRecord_AllProperties_CanBeSet()
    {
        var record = new SoaRecord
        {
            nsname = "ns.example.com",
            hostmaster = "root.example.com",
            serial = 2013101809,
            refresh = 10000,
            retry = 2400,
            expire = 604800,
            minttl = 3600
        };

        Assert.Equal("ns.example.com", record.nsname);
        Assert.Equal("root.example.com", record.hostmaster);
        Assert.Equal(2013101809, record.serial);
        Assert.Equal(10000, record.refresh);
        Assert.Equal(2400, record.retry);
        Assert.Equal(604800, record.expire);
        Assert.Equal(3600, record.minttl);
    }
}
