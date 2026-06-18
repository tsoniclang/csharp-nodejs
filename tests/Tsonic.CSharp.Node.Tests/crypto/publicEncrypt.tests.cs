using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class publicEncryptTests
{
    [Fact]
    public void publicEncrypt_PrivateDecrypt_RoundTrip()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("rsa", null);
        var plaintext = Encoding.UTF8.GetBytes("Hello, World!");

        var encrypted = crypto.publicEncrypt(publicKey, plaintext);
        var decrypted = crypto.privateDecrypt(privateKey, encrypted);

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void publicEncrypt_PrivateDecrypt_WithStringKey_Works()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("rsa", null);
        var publicPem = publicKey.export().ToString()!;
        var privatePem = privateKey.export().ToString()!;

        var plaintext = Encoding.UTF8.GetBytes("Hello, World!");
        var encrypted = crypto.publicEncrypt(publicPem, plaintext);
        var decrypted = crypto.privateDecrypt(privatePem, encrypted);

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void publicEncrypt_PrivateDecrypt_WithKeyObject_Works()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("rsa", null);

        var plaintext = Encoding.UTF8.GetBytes("Hello, World!");
        var encrypted = crypto.publicEncrypt(publicKey, plaintext);
        var decrypted = crypto.privateDecrypt(privateKey, encrypted);

        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void publicEncrypt_InvalidKeyType_Throws()
    {
        var (_, privateKey) = crypto.generateKeyPairSync("rsa", null);
        var plaintext = Encoding.UTF8.GetBytes("test");

        Assert.Throws<ArgumentException>(() => crypto.publicEncrypt(privateKey, plaintext));
    }
}
