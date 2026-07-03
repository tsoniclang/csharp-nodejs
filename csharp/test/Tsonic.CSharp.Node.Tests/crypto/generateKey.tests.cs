using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class generateKeyTests
{
    [Fact]
    public void generateKey_AES256_GeneratesKey()
    {
        var key = crypto.generateKey("aes-256-cbc", new { length = 256 });

        Assert.NotNull(key);
        Assert.Equal("secret", key.type);
        Assert.Equal(32, key.symmetricKeySize); // 256 bits = 32 bytes
    }

    [Fact]
    public void generateKey_AES128_GeneratesKey()
    {
        var key = crypto.generateKey("aes-128-cbc", new { length = 128 });

        Assert.NotNull(key);
        Assert.Equal("secret", key.type);
        Assert.Equal(16, key.symmetricKeySize); // 128 bits = 16 bytes
    }

    [Fact]
    public void generateKey_Callback_Works()
    {
        Exception? caughtError = null;
        KeyObject? resultKey = null;
        crypto.generateKey("aes", new { length = 256 }, (err, key) =>
        {
            caughtError = err;
            resultKey = key;
        });

        Assert.Null(caughtError);
        Assert.NotNull(resultKey);
        Assert.Equal("secret", resultKey.type);
    }
}
