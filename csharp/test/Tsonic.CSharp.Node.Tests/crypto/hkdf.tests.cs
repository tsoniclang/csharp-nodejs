using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class hkdfTests
{
    [Fact]
    public void hkdf_Callback_DerivesKey()
    {
        Exception? caughtError = null;
        byte[]? resultKey = null;
        crypto.hkdf("sha256", new byte[32], new byte[16], new byte[16], 32, (err, key) =>
        {
            caughtError = err;
            resultKey = key;
        });

        Assert.Null(caughtError);
        Assert.NotNull(resultKey);
        Assert.Equal(32, resultKey.Length);
    }
}
