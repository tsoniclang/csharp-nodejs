using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class hkdfSyncTests
{
    [Fact]
    public void hkdfSync_DerivesKey()
    {
        var ikm = new byte[32];
        var salt = new byte[16];
        var info = new byte[16];

        var key = crypto.hkdfSync("sha256", ikm, salt, info, 32);

        Assert.NotNull(key);
        Assert.Equal(32, key.Length);
    }
}
