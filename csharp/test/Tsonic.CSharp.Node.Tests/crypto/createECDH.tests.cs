using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class createECDHTests
{
    [Fact]
    public void createECDH_GeneratesKeys()
    {
        var ecdh = crypto.createECDH("secp256r1");
        var publicKey = ecdh.generateKeys();

        Assert.NotEmpty(publicKey);
    }

    [Fact]
    public void createECDH_ComputesSharedSecret()
    {
        var alice = crypto.createECDH("secp256r1");
        var bob = crypto.createECDH("secp256r1");

        alice.generateKeys();
        bob.generateKeys();

        var aliceSecret = alice.computeSecret(bob.getPublicKey());
        var bobSecret = bob.computeSecret(alice.getPublicKey());

        Assert.Equal(aliceSecret, bobSecret);
    }

    [Fact]
    public void createECDH_GetPublicKey()
    {
        var ecdh = crypto.createECDH("secp256r1");
        ecdh.generateKeys();
        var publicKey = ecdh.getPublicKey();

        Assert.NotEmpty(publicKey);
    }

    [Fact]
    public void createECDH_GetPrivateKey()
    {
        var ecdh = crypto.createECDH("secp256r1");
        ecdh.generateKeys();
        var privateKey = ecdh.getPrivateKey();

        Assert.NotEmpty(privateKey);
    }

    [Fact]
    public void createECDH_WithEncodedKeys()
    {
        var ecdh = crypto.createECDH("secp256r1");
        var publicKeyHex = ecdh.generateKeys("hex");

        Assert.NotEmpty(publicKeyHex);
    }

    [Fact]
    public void createECDH_SharedSecret_WithEncoding()
    {
        var alice = crypto.createECDH("secp256r1");
        var bob = crypto.createECDH("secp256r1");

        alice.generateKeys();
        bob.generateKeys();

        var aliceSecretHex = alice.computeSecret(bob.getPublicKey(), "hex");
        var bobSecretHex = bob.computeSecret(alice.getPublicKey(), "hex");

        Assert.Equal(aliceSecretHex, bobSecretHex);
    }

    [Fact]
    public void createECDH_secp384r1_Curve()
    {
        var ecdh = crypto.createECDH("secp384r1");
        var publicKey = ecdh.generateKeys();
        Assert.NotEmpty(publicKey);
    }

    [Fact]
    public void createECDH_secp521r1_Curve()
    {
        var ecdh = crypto.createECDH("secp521r1");
        var publicKey = ecdh.generateKeys();
        Assert.NotEmpty(publicKey);
    }

    [Fact]
    public void createECDH_secp256k1_Curve()
    {
        var ecdh = crypto.createECDH("secp256k1");
        var publicKey = ecdh.generateKeys();
        Assert.NotEmpty(publicKey);
    }

    [Fact]
    public void createECDH_secp256k1_SharedSecret()
    {
        var alice = crypto.createECDH("secp256k1");
        var bob = crypto.createECDH("secp256k1");

        var alicePublic = alice.generateKeys();
        var bobPublic = bob.generateKeys();

        var aliceShared = alice.computeSecret(bobPublic);
        var bobShared = bob.computeSecret(alicePublic);

        Assert.Equal(aliceShared, bobShared);
    }

    [Fact]
    public void createECDH_SetPublicKey_ThrowsNotSupported()
    {
        var ecdh = crypto.createECDH("secp256r1");
        var publicKey = new byte[65];

        // ECDH.setPublicKey is not supported
        Assert.Throws<NotSupportedException>(() => ecdh.setPublicKey(publicKey));
    }

    [Fact]
    public void createECDH_SetPrivateKey_Works()
    {
        var ecdh1 = crypto.createECDH("secp256r1");
        ecdh1.generateKeys();
        var privateKey = ecdh1.getPrivateKey();

        var ecdh2 = crypto.createECDH("secp256r1");
        ecdh2.setPrivateKey(privateKey);
        var retrieved = ecdh2.getPrivateKey();

        Assert.Equal(privateKey, retrieved);
    }

    [Fact]
    public void createECDH_InvalidCurve_Throws()
    {
        Assert.Throws<ArgumentException>(() => crypto.createECDH("invalid-curve"));
    }
}
