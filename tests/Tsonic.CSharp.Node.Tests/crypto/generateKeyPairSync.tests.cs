using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class generateKeyPairSyncTests
{
    [Fact]
    public void generateKeyPairSync_RSA_CreatesValidKeyPair()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("rsa", null);

        Assert.NotNull(publicKey);
        Assert.NotNull(privateKey);
        Assert.Equal("public", publicKey.type);
        Assert.Equal("private", privateKey.type);
        Assert.Equal("rsa", publicKey.asymmetricKeyType);
        Assert.Equal("rsa", privateKey.asymmetricKeyType);
    }

    [Fact]
    public void generateKeyPairSync_EC_CreatesValidKeyPair()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("ec", null);

        Assert.NotNull(publicKey);
        Assert.NotNull(privateKey);
        Assert.Equal("public", publicKey.type);
        Assert.Equal("private", privateKey.type);
        Assert.Equal("ec", publicKey.asymmetricKeyType);
        Assert.Equal("ec", privateKey.asymmetricKeyType);
    }

    [Fact]
    public void generateKeyPairSync_Ed25519_GeneratesKeys()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("ed25519", null);

        Assert.NotNull(publicKey);
        Assert.NotNull(privateKey);
        Assert.Equal("public", publicKey.type);
        Assert.Equal("private", privateKey.type);
        Assert.Equal("ed25519", publicKey.asymmetricKeyType);
        Assert.Equal("ed25519", privateKey.asymmetricKeyType);
    }

    [Fact]
    public void generateKeyPairSync_DSA_GeneratesKeys()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("dsa", null);

        Assert.NotNull(publicKey);
        Assert.NotNull(privateKey);
        Assert.Equal("public", publicKey.type);
        Assert.Equal("private", privateKey.type);
        Assert.Equal("dsa", publicKey.asymmetricKeyType);
        Assert.Equal("dsa", privateKey.asymmetricKeyType);
    }

    [Fact]
    public void generateKeyPairSync_DH_GeneratesKeys()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("dh", null);

        Assert.NotNull(publicKey);
        Assert.NotNull(privateKey);
        Assert.Equal("secret", publicKey.type);
        Assert.Equal("secret", privateKey.type);
    }

    [Fact]
    public void generateKeyPairSync_DH_GeneratesKeys_Reliably()
    {
        for (var i = 0; i < 8; i++)
        {
            var (publicKey, privateKey) = crypto.generateKeyPairSync("dh", null);

            Assert.NotNull(publicKey);
            Assert.NotNull(privateKey);
            Assert.Equal("secret", publicKey.type);
            Assert.Equal("secret", privateKey.type);
        }
    }

    [Fact]
    public void generateKeyPairSync_InvalidType_Throws()
    {
        Assert.Throws<ArgumentException>(() => crypto.generateKeyPairSync("invalid", null));
    }
}
