using Xunit;
using System;
using System.Security.Cryptography;

namespace Tsonic.CSharp.Node.Tests;

public class privateEncryptTests
{
    [Fact]
    public void privateEncrypt_PublicDecrypt_Roundtrip()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("rsa", null);
        var publicPem = publicKey.export().ToString()!;
        var privatePem = privateKey.export().ToString()!;

        var plaintext = new byte[32];
        RandomNumberGenerator.Fill(plaintext);

        // Private encrypt then public decrypt
        var encrypted = crypto.privateEncrypt(privatePem, plaintext);
        var decrypted = crypto.publicDecrypt(publicPem, encrypted);

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void privateEncrypt_PublicDecrypt_WithKeyObjects()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("rsa", null);

        var plaintext = new byte[32];
        RandomNumberGenerator.Fill(plaintext);

        // Private encrypt then public decrypt
        var encrypted = crypto.privateEncrypt(privateKey, plaintext);
        var decrypted = crypto.publicDecrypt(publicKey, encrypted);

        Assert.Equal(plaintext, decrypted);
    }
}
