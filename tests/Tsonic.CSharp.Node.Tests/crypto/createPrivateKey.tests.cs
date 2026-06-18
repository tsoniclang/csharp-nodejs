using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class createPrivateKeyTests
{
    [Fact]
    public void createPrivateKey_FromBytes_Works()
    {
        var (_, privateKey) = crypto.generateKeyPairSync("rsa", null);
        var exported = ((PrivateKeyObject)privateKey).export("pem", "pkcs8");

        var privateRsa = System.Security.Cryptography.RSA.Create();
        privateRsa.ImportFromPem(exported);
        var derBytes = privateRsa.ExportPkcs8PrivateKey();

        var keyObj = crypto.createPrivateKey(derBytes);
        Assert.Equal("private", keyObj.type);
        Assert.Equal("rsa", keyObj.asymmetricKeyType);
    }
}
