using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class createDiffieHellmanTests
{
    [Fact]
    public void createDiffieHellman_WithPrimeAndGenerator()
    {
        var prime = crypto.randomBytes(256);
        var generator = 2;

        var dh = crypto.createDiffieHellman(prime, generator);
        var publicKey = dh.generateKeys();

        Assert.NotEmpty(publicKey);
    }

    [Fact]
    public void createDiffieHellman_GetPrime()
    {
        var prime = crypto.randomBytes(128);
        var dh = crypto.createDiffieHellman(prime, 2);

        var retrievedPrime = dh.getPrime();
        Assert.Equal(prime, retrievedPrime);
    }

    [Fact]
    public void createDiffieHellman_GetGenerator()
    {
        var prime = crypto.randomBytes(128);
        var dh = crypto.createDiffieHellman(prime, 2);

        var generator = dh.getGenerator();
        Assert.NotEmpty(generator);
    }

    [Fact]
    public void createDiffieHellman_GetPublicKey()
    {
        var prime = crypto.randomBytes(128);
        var dh = crypto.createDiffieHellman(prime, 2);

        dh.generateKeys();
        var publicKey = dh.getPublicKey();

        Assert.NotEmpty(publicKey);
    }

    [Fact]
    public void createDiffieHellman_GetPrivateKey()
    {
        var prime = crypto.randomBytes(128);
        var dh = crypto.createDiffieHellman(prime, 2);

        dh.generateKeys();
        var privateKey = dh.getPrivateKey();

        Assert.NotEmpty(privateKey);
    }

    [Fact]
    public void createDiffieHellman_SetGetKeys()
    {
        var prime = crypto.randomBytes(128);
        var dh = crypto.createDiffieHellman(prime, 2);

        dh.generateKeys();
        var publicKey = dh.getPublicKey();
        var privateKey = dh.getPrivateKey();

        var dh2 = crypto.createDiffieHellman(prime, 2);
        dh2.setPrivateKey(privateKey);

        Assert.Equal(publicKey, dh2.getPublicKey());
    }

    [Fact]
    public void createDiffieHellman_WithEncodedKeys()
    {
        var prime = crypto.randomBytes(128);
        var dh = crypto.createDiffieHellman(prime, 2);

        var publicKeyHex = dh.generateKeys("hex");
        Assert.NotEmpty(publicKeyHex);

        var publicKeyBase64 = dh.getPublicKey("base64");
        Assert.NotEmpty(publicKeyBase64);
    }

    [Fact]
    public void createDiffieHellman_WithGeneratedPrime_Works()
    {
        // DiffieHellman with auto-generated prime
        var dh = crypto.createDiffieHellman(512); // Use 512 for faster test

        var prime = dh.getPrime();
        var generator = dh.getGenerator();

        Assert.NotNull(prime);
        Assert.NotNull(generator);
        Assert.True(prime.Length > 0);
        Assert.True(generator.Length > 0);

        // Should be able to generate keys
        var publicKey = dh.generateKeys();
        Assert.NotNull(publicKey);
        Assert.True(publicKey.Length > 0);
    }
}
