using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class AddressInfoTests
{
    [Fact]
    public void AddressInfo_AllProperties_CanBeSet()
    {
        var addressInfo = new AddressInfo
        {
            address = "192.168.1.1",
            family = "IPv4",
            port = 8080
        };

        Assert.Equal("192.168.1.1", addressInfo.address);
        Assert.Equal("IPv4", addressInfo.family);
        Assert.Equal(8080, addressInfo.port);
    }
}
