using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class EphemeralKeyInfoTests
{
    [Fact]
    public void EphemeralKeyInfo_AllProperties_CanBeSet()
    {
        var info = new EphemeralKeyInfo
        {
            type = "ECDH",
            name = "prime256v1",
            size = 256
        };

        Assert.Equal("ECDH", info.type);
        Assert.Equal("prime256v1", info.name);
        Assert.Equal(256, info.size);
    }
}
