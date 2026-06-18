using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class generateKeyPairTests
{
    [Fact]
    public void generateKeyPair_Callback_Works()
    {
        object? pubKey = null;
        object? privKey = null;
        Exception? error = null;

        crypto.generateKeyPair("rsa", null, (err, pub, priv) =>
        {
            error = err;
            pubKey = pub;
            privKey = priv;
        });

        Assert.Null(error);
        Assert.NotNull(pubKey);
        Assert.NotNull(privKey);
        Assert.IsType<PublicKeyObject>(pubKey);
        Assert.IsType<PrivateKeyObject>(privKey);
    }
}
