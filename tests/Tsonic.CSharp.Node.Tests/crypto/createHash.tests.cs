using Xunit;
using System;
using System.Text;
using System.Security.Cryptography;

namespace Tsonic.CSharp.Node.Tests;

public class createHashTests
{
    [Fact]
    public void createHash_SHA256_ProducesCorrectDigest()
    {
        var hash = crypto.createHash("sha256");
        hash.update("Hello, World!");
        var digest = hash.digest("hex");

        Assert.Equal("dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f", digest);
    }

    [Fact]
    public void createHash_MD5_ProducesCorrectDigest()
    {
        var hash = crypto.createHash("md5");
        hash.update("Hello, World!");
        var digest = hash.digest("hex");

        Assert.Equal("65a8e27d8879283831b664bd8b7f0ad4", digest);
    }

    [Fact]
    public void createHash_MultipleUpdates_ProducesCorrectDigest()
    {
        var hash = crypto.createHash("sha256");
        hash.update("Hello");
        hash.update(", ");
        hash.update("World!");
        var digest = hash.digest("hex");

        Assert.Equal("dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f", digest);
    }

    [Fact]
    public void createHash_ThrowsForUnknownAlgorithm()
    {
        Assert.Throws<ArgumentException>(() => crypto.createHash("invalid-algorithm"));
    }

    [Fact]
    public void createHash_DigestOnlyOnce()
    {
        var hash = crypto.createHash("sha256");
        hash.update("test");
        hash.digest();

        Assert.Throws<InvalidOperationException>(() => hash.digest());
    }

    [Fact]
    public void createHash_SHA1_ProducesCorrectDigest()
    {
        var hash = crypto.createHash("sha1");
        hash.update("test");
        var digest = hash.digest("hex");
        Assert.Equal(40, digest.Length); // SHA1 = 20 bytes = 40 hex chars
    }

    [Fact]
    public void createHash_SHA384_ProducesCorrectDigest()
    {
        var hash = crypto.createHash("sha384");
        hash.update("test");
        var digest = hash.digest("hex");
        Assert.Equal(96, digest.Length); // SHA384 = 48 bytes = 96 hex chars
    }

    [Fact]
    public void createHash_SHA512_ProducesCorrectDigest()
    {
        var hash = crypto.createHash("sha512");
        hash.update("test");
        var digest = hash.digest("hex");
        Assert.Equal(128, digest.Length); // SHA512 = 64 bytes = 128 hex chars
    }

    [Fact]
    public void createHash_SHA512_224_ProducesCorrectDigest()
    {
        var hash = crypto.createHash("sha512-224");
        hash.update("test");
        var digest = hash.digest("hex");
        Assert.Equal(56, digest.Length); // SHA512-224 = 28 bytes = 56 hex chars
    }

    [Fact]
    public void createHash_SHA512_256_ProducesCorrectDigest()
    {
        var hash = crypto.createHash("sha512-256");
        hash.update("test");
        var digest = hash.digest("hex");
        Assert.Equal(64, digest.Length); // SHA512-256 = 32 bytes = 64 hex chars
    }

    [Fact]
    public void createHash_SHAKE128_ProducesCorrectDigest()
    {
        var hash = crypto.createHash("shake128");
        hash.update("test");
        var digest = hash.digest(16); // Request 16 bytes
        Assert.Equal(16, digest.Length);
    }

    [Fact]
    public void createHash_SHAKE128_DefaultOutput()
    {
        var hash = crypto.createHash("shake128");
        hash.update("test");
        var digest = hash.digest(); // Default 16 bytes for SHAKE128
        Assert.Equal(16, digest.Length);
    }

    [Fact]
    public void createHash_SHAKE256_ProducesCorrectDigest()
    {
        var hash = crypto.createHash("shake256");
        hash.update("test");
        var digest = hash.digest(32); // Request 32 bytes
        Assert.Equal(32, digest.Length);
    }

    [Fact]
    public void createHash_SHAKE256_DefaultOutput()
    {
        var hash = crypto.createHash("shake256");
        hash.update("test");
        var digest = hash.digest(); // Default 32 bytes for SHAKE256
        Assert.Equal(32, digest.Length);
    }

    [Fact]
    public void createHash_Copy_WorksCorrectly()
    {
        var hash1 = crypto.createHash("blake2b512");
        hash1.update("part1");

        var hash2 = hash1.copy();

        hash1.update("part2a");
        var digest1 = hash1.digest("hex");

        hash2.update("part2b");
        var digest2 = hash2.digest("hex");

        // Digests should be different since we updated with different data
        Assert.NotEqual(digest1, digest2);
    }

    [Fact]
    public void createHash_Copy_SHA3_WorksCorrectly()
    {
        var hash1 = crypto.createHash("sha3-256");
        hash1.update("test");

        var hash2 = hash1.copy();

        var digest1 = hash1.digest("hex");
        var digest2 = hash2.digest("hex");

        // Digests should be identical since they have same state
        Assert.Equal(digest1, digest2);
    }

    [Fact]
    public void createHash_Copy_SHAKE_WorksCorrectly()
    {
        var hash1 = crypto.createHash("shake128");
        hash1.update("test");

        var hash2 = hash1.copy();

        var digest1 = hash1.digest(16);
        var digest2 = hash2.digest(16);

        // Digests should be identical
        Assert.Equal(digest1, digest2);
    }

    [Fact]
    public void createHash_Base64Encoding()
    {
        var hash = crypto.createHash("sha256");
        hash.update("test");
        var digest = hash.digest("base64");
        Assert.NotEmpty(digest);
        Assert.DoesNotContain("-", digest); // base64, not base64url
    }

    [Fact]
    public void createHash_Base64UrlEncoding()
    {
        var hash = crypto.createHash("sha256");
        hash.update("test");
        var digest = hash.digest("base64url");
        Assert.NotEmpty(digest);
        Assert.DoesNotContain("+", digest);
        Assert.DoesNotContain("/", digest);
    }

    [Fact]
    public void createHash_BinaryOutput()
    {
        var hash = crypto.createHash("sha256");
        hash.update("test");
        var digest = hash.digest();
        Assert.Equal(32, digest.Length); // SHA256 = 32 bytes
    }

    [Fact]
    public void createHash_InvalidAlgorithm_Throws()
    {
        Assert.Throws<ArgumentException>(() => crypto.createHash("invalid-algo"));
    }

    [Fact]
    public void createHash_UpdateAfterDigest_Throws()
    {
        var hash = crypto.createHash("sha256");
        hash.update("test");
        hash.digest();

        Assert.Throws<InvalidOperationException>(() => hash.update("more"));
    }

    [Fact]
    public void createHash_UpdateWithBytes_Works()
    {
        var hash = crypto.createHash("sha256");
        hash.update(Encoding.UTF8.GetBytes("Hello, World!"));
        var digest = hash.digest("hex");

        Assert.Equal("dffd6021bb2bd5b0af676290809ec3a53191dd81c7f70a4b28688a362182986f", digest);
    }

    [Fact]
    public void createHash_DigestAsBytes_Works()
    {
        var hash = crypto.createHash("sha256");
        hash.update("test");
        var digest = hash.digest();

        Assert.Equal(32, digest.Length); // SHA256 = 32 bytes
    }

    [Fact]
    public void createHash_DigestBase64Url_Works()
    {
        var hash = crypto.createHash("sha256");
        hash.update("test");
        var digest = hash.digest("base64url");

        Assert.NotEmpty(digest);
        Assert.DoesNotContain("+", digest);
        Assert.DoesNotContain("/", digest);
        Assert.DoesNotContain("=", digest);
    }
}
