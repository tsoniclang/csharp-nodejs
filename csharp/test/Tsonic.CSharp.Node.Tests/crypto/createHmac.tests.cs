using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class createHmacTests
{
    [Fact]
    public void createHmac_SHA256_ProducesCorrectDigest()
    {
        var hmac = crypto.createHmac("sha256", "secret-key");
        hmac.update("Hello, World!");
        var digest = hmac.digest("hex");

        Assert.NotEmpty(digest);
        Assert.Equal(64, digest.Length); // SHA256 produces 32 bytes = 64 hex characters
    }

    [Fact]
    public void createHmac_MD5_Works()
    {
        var hmac = crypto.createHmac("md5", "key");
        hmac.update("data");
        var digest = hmac.digest("hex");
        Assert.Equal(32, digest.Length); // MD5 = 16 bytes = 32 hex chars
    }

    [Fact]
    public void createHmac_SHA1_Works()
    {
        var hmac = crypto.createHmac("sha1", "key");
        hmac.update("data");
        var digest = hmac.digest("hex");
        Assert.Equal(40, digest.Length); // SHA1 = 20 bytes = 40 hex chars
    }

    [Fact]
    public void createHmac_SHA384_Works()
    {
        var hmac = crypto.createHmac("sha384", "key");
        hmac.update("data");
        var digest = hmac.digest("hex");
        Assert.Equal(96, digest.Length);
    }

    [Fact]
    public void createHmac_SHA512_Works()
    {
        var hmac = crypto.createHmac("sha512", "key");
        hmac.update("data");
        var digest = hmac.digest("hex");
        Assert.Equal(128, digest.Length);
    }

    [Fact]
    public void createHmac_WithByteArrayKey()
    {
        var key = Encoding.UTF8.GetBytes("secret-key");
        var hmac = crypto.createHmac("sha256", key);
        hmac.update("data");
        var digest = hmac.digest("hex");
        Assert.NotEmpty(digest);
    }

    [Fact]
    public void createHmac_BinaryOutput()
    {
        var hmac = crypto.createHmac("sha256", "key");
        hmac.update("data");
        var digest = hmac.digest();
        Assert.Equal(32, digest.Length);
    }

    [Fact]
    public void createHmac_InvalidAlgorithm_Throws()
    {
        Assert.Throws<ArgumentException>(() => crypto.createHmac("invalid-algo", "key"));
    }

    [Fact]
    public void createHmac_UpdateWithBytes_Works()
    {
        var hmac = crypto.createHmac("sha256", new byte[] { 1, 2, 3, 4 });
        hmac.update(Encoding.UTF8.GetBytes("test"));
        var digest = hmac.digest("hex");

        Assert.NotEmpty(digest);
        Assert.Equal(64, digest.Length);
    }

    [Fact]
    public void createHmac_DigestAsBytes_Works()
    {
        var hmac = crypto.createHmac("sha256", "key");
        hmac.update("test");
        var digest = hmac.digest();

        Assert.Equal(32, digest.Length);
    }
}
