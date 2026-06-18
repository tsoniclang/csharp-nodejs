using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class scryptSyncTests
{
    [Fact]
    public void scryptSync_GeneratesKey()
    {
        var key = crypto.scryptSync("password", "salt", 32, null);

        Assert.NotNull(key);
        Assert.Equal(32, key.Length);
    }
}
