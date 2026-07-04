using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class LookupAddressTests
{
    [Fact]
    public void LookupAddress_AllProperties_CanBeSet()
    {
        var address = new LookupAddress
        {
            address = "127.0.0.1",
            family = 4
        };

        Assert.Equal("127.0.0.1", address.address);
        Assert.Equal(4, address.family);
    }
}
