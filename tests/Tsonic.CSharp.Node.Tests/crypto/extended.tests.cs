using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class CryptoExtendedTests
{
    [Fact]
    public void DigestAndHmacDigest_ShouldReturnEncodedResults()
    {
        var digest = crypto.digest("sha256", Encoding.UTF8.GetBytes("abc"));
        var hmac = crypto.hmacDigest("sha256", Encoding.UTF8.GetBytes("key"), Encoding.UTF8.GetBytes("abc"));

        Assert.Equal("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", digest.hex);
        Assert.NotEmpty(hmac.hex);
        Assert.Equal(32, hmac.bytes.Length);
    }

    [Fact]
    public void Aes256Gcm_ShouldEncryptAndDecrypt()
    {
        var key = crypto.randomBytes(32);
        var nonce = crypto.randomBytes(12);
        var plaintext = Encoding.UTF8.GetBytes("secret");

        var encrypted = crypto.aes256GcmEncrypt(key, nonce, plaintext);
        var decrypted = crypto.aes256GcmDecrypt(key, nonce, encrypted.ciphertext, encrypted.authTag);

        Assert.NotEqual(plaintext, encrypted.ciphertext);
        Assert.Equal(plaintext, decrypted);
    }

    [Fact]
    public void RsaHelpers_ShouldGenerateSignVerifyAndCompareKeys()
    {
        var pair = crypto.generateRsaKeyPair();
        var data = Encoding.UTF8.GetBytes("payload");
        var signature = crypto.signSha256(data, pair.privateKey);

        Assert.True(crypto.verifySha256(data, pair.publicKey, signature));
        Assert.True(pair.publicKey.equals(pair.publicKey));
        Assert.NotNull(pair.publicKey.asymmetricKeyDetails);
    }

    [Fact]
    public void ConstantsSecureHeapCipherInfoAndWebCrypto_ShouldBeAvailable()
    {
        var heap = crypto.secureHeapUsed();
        var cipher = crypto.getCipherInfo("aes-256-gcm");
        var key = crypto.webcrypto.subtle.importKey("raw", [1, 2, 3], new KeyAlgorithm { name = "raw" }, true, ["sign"]);

        Assert.Equal(1, crypto.constants.RSA_PKCS1_PADDING);
        Assert.Equal(0, heap.used);
        Assert.NotNull(cipher);
        Assert.Equal(32, cipher!.keyLength);
        Assert.Equal([1, 2, 3], crypto.webcrypto.subtle.exportKey("raw", key));
    }

    [Fact]
    public void X509Certificate_ShouldExposeLegacyAndJsonViews()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=example.test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        var x509 = new X509Certificate(certificate);

        Assert.Contains("CN=example.test", x509.subject);
        Assert.Contains("BEGIN CERTIFICATE", x509.toJSON());
        Assert.Contains("CN=example.test", x509.toLegacyObject().subject);
    }
}
