using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class createSecretKeyTests
{
    [Fact]
    public void createSecretKey_CreatesKeyObject()
    {
        var key = crypto.createSecretKey(crypto.randomBytes(32));

        Assert.Equal("secret", key.type);
        Assert.Equal(32, key.symmetricKeySize);
        Assert.Null(key.asymmetricKeyType);
    }

    [Fact]
    public void createSecretKey_WithString()
    {
        var key = crypto.createSecretKey("my-secret-key", "utf8");
        Assert.Equal("secret", key.type);
        Assert.Null(key.asymmetricKeyType);
    }

    [Fact]
    public void createSecretKey_ExportWorks()
    {
        var originalKey = crypto.randomBytes(32);
        var keyObj = crypto.createSecretKey(originalKey);
        var exported = ((SecretKeyObject)keyObj).export();

        Assert.Equal(originalKey, exported);
    }

    [Fact]
    public void createSecretKey_WithHexEncoding_Works()
    {
        var hexKey = "0123456789abcdef";
        var keyObj = crypto.createSecretKey(hexKey, "hex");

        Assert.Equal("secret", keyObj.type);
        Assert.Equal(8, keyObj.symmetricKeySize);
    }

    [Fact]
    public void createSecretKey_WithBase64Encoding_Works()
    {
        var base64Key = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });
        var keyObj = crypto.createSecretKey(base64Key, "base64");

        Assert.Equal("secret", keyObj.type);
        Assert.Equal(8, keyObj.symmetricKeySize);
    }

    [Fact]
    public void createSecretKey_WithBase64UrlEncoding_Works()
    {
        // Base64url uses - and _ instead of + and /, padding removed
        // AQIDBAUG = [1,2,3,4,5,6] in base64, remove padding for base64url
        var base64url = "AQIDBAUG"; // No padding
        var keyObj = crypto.createSecretKey(base64url, "base64url");

        Assert.Equal("secret", keyObj.type);
        Assert.Equal(6, keyObj.symmetricKeySize);
    }
}
