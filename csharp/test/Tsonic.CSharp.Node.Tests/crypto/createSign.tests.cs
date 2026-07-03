using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class createSignTests
{
    [Fact]
    public void createSign_RSA_SHA256()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("rsa", null);
        var data = Encoding.UTF8.GetBytes("Important message");

        var sign = crypto.createSign("sha256");
        sign.update(data);
        var signature = sign.sign(privateKey);

        Assert.NotEmpty(signature);
    }

    [Fact]
    public void createSign_Verify_WithStringData()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("rsa", null);

        var sign = crypto.createSign("sha256");
        sign.update("Important message");
        var signature = sign.sign(privateKey);

        var verify = crypto.createVerify("sha256");
        verify.update("Important message");
        var isValid = verify.verify(publicKey, signature);

        Assert.True(isValid);
    }

    [Fact]
    public void createSign_MultipleUpdates()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("rsa", null);

        var sign = crypto.createSign("sha256");
        sign.update("Part 1");
        sign.update(" Part 2");
        sign.update(" Part 3");
        var signature = sign.sign(privateKey);

        var verify = crypto.createVerify("sha256");
        verify.update("Part 1 Part 2 Part 3");
        var isValid = verify.verify(publicKey, signature);

        Assert.True(isValid);
    }

    [Fact]
    public void createSign_DSA_Works()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("dsa", null);

        var sign = crypto.createSign("sha256");
        sign.update("test data");
        var signature = sign.sign(privateKey);

        var verify = crypto.createVerify("sha256");
        verify.update("test data");
        var isValid = verify.verify(publicKey, signature);

        Assert.True(isValid);
    }

    [Fact]
    public void createSign_DSA_WithPEM_Works()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("dsa", null);
        var publicPem = publicKey.export().ToString()!;
        var privatePem = privateKey.export().ToString()!;

        var sign = crypto.createSign("sha256");
        sign.update("test data");
        var signature = sign.sign(privatePem, "hex");

        var verify = crypto.createVerify("sha256");
        verify.update("test data");
        var isValid = verify.verify(publicPem, signature, "hex");

        Assert.True(isValid);
    }
}
