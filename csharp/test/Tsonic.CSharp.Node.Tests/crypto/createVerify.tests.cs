using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class createVerifyTests
{
    [Fact]
    public void createVerify_RSA_SHA256()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("rsa", null);
        var data = Encoding.UTF8.GetBytes("Important message");

        var sign = crypto.createSign("sha256");
        sign.update(data);
        var signature = sign.sign(privateKey);

        var verify = crypto.createVerify("sha256");
        verify.update(data);
        var isValid = verify.verify(publicKey, signature);

        Assert.True(isValid);
    }

    [Fact]
    public void createVerify_RSA_InvalidSignature()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("rsa", null);
        var data = Encoding.UTF8.GetBytes("Important message");

        var sign = crypto.createSign("sha256");
        sign.update(data);
        var signature = sign.sign(privateKey);

        // Corrupt signature
        signature[0] ^= 0xFF;

        var verify = crypto.createVerify("sha256");
        verify.update(data);
        var isValid = verify.verify(publicKey, signature);

        Assert.False(isValid);
    }

    [Fact]
    public void createVerify_DSA_FailsWithWrongData()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("dsa", null);

        var sign = crypto.createSign("sha256");
        sign.update("test data");
        var signature = sign.sign(privateKey);

        var verify = crypto.createVerify("sha256");
        verify.update("wrong data");
        var isValid = verify.verify(publicKey, signature);

        Assert.False(isValid);
    }
}
