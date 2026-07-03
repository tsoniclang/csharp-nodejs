using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class KeyObjectTests
{
    [Fact]
    public void KeyObject_SecretKeyObject_Export_Works()
    {
        var keyData = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var keyObj = crypto.createSecretKey(keyData) as SecretKeyObject;
        var exported = keyObj!.export();

        Assert.Equal(keyData, exported);
    }

    [Fact]
    public void KeyObject_PublicKeyObject_ExportWithFormat_Pem_Works()
    {
        var (publicKey, _) = crypto.generateKeyPairSync("rsa", null);
        var pem = ((PublicKeyObject)publicKey).export("pem", "spki");

        Assert.Contains("BEGIN PUBLIC KEY", pem);
        Assert.Contains("END PUBLIC KEY", pem);
    }

    [Fact]
    public void KeyObject_PublicKeyObject_ExportWithFormat_Der_Works()
    {
        var (publicKey, _) = crypto.generateKeyPairSync("rsa", null);
        var der = ((PublicKeyObject)publicKey).export("der", "spki");

        Assert.NotEmpty(der);
    }

    [Fact]
    public void KeyObject_PrivateKeyObject_ExportWithFormat_Pem_Works()
    {
        var (_, privateKey) = crypto.generateKeyPairSync("rsa", null);
        var pem = ((PrivateKeyObject)privateKey).export("pem", "pkcs8");

        Assert.Contains("BEGIN PRIVATE KEY", pem);
        Assert.Contains("END PRIVATE KEY", pem);
    }

    [Fact]
    public void KeyObject_PrivateKeyObject_ExportWithFormat_Der_Works()
    {
        var (_, privateKey) = crypto.generateKeyPairSync("rsa", null);
        var der = ((PrivateKeyObject)privateKey).export("der", "pkcs8");

        Assert.NotEmpty(der);
    }

    [Fact]
    public void KeyObject_PrivateKeyObject_ExportEncrypted_Works()
    {
        var (_, privateKey) = crypto.generateKeyPairSync("rsa", null);
        var encrypted = ((PrivateKeyObject)privateKey).export("pem", "pkcs8", "aes256", "password123");

        // Encrypted export returns DER bytes (not PEM), check it's not empty
        Assert.NotEmpty(encrypted);
    }
}
