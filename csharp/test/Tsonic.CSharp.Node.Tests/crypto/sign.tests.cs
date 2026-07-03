using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class signTests
{
    [Fact]
    public void sign_StaticSign_Works()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("rsa", null);
        var data = Encoding.UTF8.GetBytes("Test data");

        var signature = crypto.sign("sha256", data, privateKey);
        Assert.NotEmpty(signature);
    }

    [Fact]
    public void sign_StaticVerify_Works()
    {
        var (publicKey, privateKey) = crypto.generateKeyPairSync("rsa", null);
        var data = Encoding.UTF8.GetBytes("Test data");

        var signature = crypto.sign("sha256", data, privateKey);
        var isValid = crypto.verify("sha256", data, publicKey, signature);

        Assert.True(isValid);
    }
}
