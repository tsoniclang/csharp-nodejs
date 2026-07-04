using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class scryptTests
{
    [Fact]
    public void scrypt_Callback_GeneratesKey()
    {
        Exception? caughtError = null;
        byte[]? resultKey = null;
        crypto.scrypt("password", "salt", 32, null, (err, key) =>
        {
            caughtError = err;
            resultKey = key;
        });

        Assert.Null(caughtError);
        Assert.NotNull(resultKey);
        Assert.Equal(32, resultKey.Length);
    }
}
