using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class createPublicKeyTests
{
    [Fact]
    public void createPublicKey_FromKeyObject()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("rsa", null);
        var extractedPublic = crypto.createPublicKey(privateKey);

        Assert.Equal("public", extractedPublic.type);
    }

    [Fact]
    public void createPublicKey_FromBytes_Works()
    {
        var (publicKey, _) = crypto.generateKeyPairSync("rsa", null);
        var exported = ((PublicKeyObject)publicKey).export("pem", "spki");
        var bytes = Encoding.UTF8.GetBytes(exported);

        // Export as DER
        var publicRsa = System.Security.Cryptography.RSA.Create();
        publicRsa.ImportFromPem(exported);
        var derBytes = publicRsa.ExportSubjectPublicKeyInfo();

        var keyObj = crypto.createPublicKey(derBytes);
        Assert.Equal("public", keyObj.type);
        Assert.Equal("rsa", keyObj.asymmetricKeyType);
    }

    [Fact]
    public void createPublicKey_FromPrivateKey_Works()
    {
        var (_, privateKey) = crypto.generateKeyPairSync("rsa", null);
        var publicKey = crypto.createPublicKey(privateKey);

        Assert.Equal("public", publicKey.type);
        Assert.Equal("rsa", publicKey.asymmetricKeyType);
    }

    [Fact]
    public void createPublicKey_FromPublicKey_ReturnsSame()
    {
        var (publicKey, _) = crypto.generateKeyPairSync("rsa", null);
        var result = crypto.createPublicKey(publicKey);

        Assert.Same(publicKey, result);
    }
}
