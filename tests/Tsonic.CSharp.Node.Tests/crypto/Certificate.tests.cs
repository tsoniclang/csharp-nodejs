using Xunit;
using System.Text;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Tsonic.CSharp.Node.Tests;

public class CertificateTests
{
    [Fact]
    public void Certificate_ExportChallenge_String_ReturnsChallengeBytes()
    {
        var spkac = CreateSignedPublicKeyAndChallenge("node-challenge");

        var challenge = Certificate.exportChallenge(Convert.ToBase64String(spkac));

        Assert.Equal("node-challenge", Encoding.UTF8.GetString(challenge));
    }

    [Fact]
    public void Certificate_ExportChallenge_Bytes_ReturnsChallengeBytes()
    {
        var spkac = CreateSignedPublicKeyAndChallenge("byte-challenge");

        var challenge = Certificate.exportChallenge(spkac);

        Assert.Equal("byte-challenge", Encoding.UTF8.GetString(challenge));
    }

    [Fact]
    public void Certificate_ExportPublicKey_String_ReturnsSubjectPublicKeyInfo()
    {
        var spkac = CreateSignedPublicKeyAndChallenge("public-key");

        var publicKey = Certificate.exportPublicKey(Convert.ToBase64String(spkac));

        Assert.NotEmpty(publicKey);
        Assert.NotNull(SubjectPublicKeyInfo.GetInstance(Asn1Object.FromByteArray(publicKey)));
    }

    [Fact]
    public void Certificate_ExportPublicKey_Bytes_ReturnsSubjectPublicKeyInfo()
    {
        var spkac = CreateSignedPublicKeyAndChallenge("public-key-bytes");

        var publicKey = Certificate.exportPublicKey(spkac);

        Assert.NotEmpty(publicKey);
        Assert.NotNull(SubjectPublicKeyInfo.GetInstance(Asn1Object.FromByteArray(publicKey)));
    }

    [Fact]
    public void Certificate_VerifySpkac_String_ReturnsTrueForValidSignature()
    {
        var spkac = CreateSignedPublicKeyAndChallenge("verify-string");

        Assert.True(Certificate.verifySpkac(Convert.ToBase64String(spkac)));
    }

    [Fact]
    public void Certificate_VerifySpkac_Bytes_ReturnsFalseForTamperedSignature()
    {
        var spkac = CreateSignedPublicKeyAndChallenge("verify-bytes");
        spkac[^1] ^= 0x01;

        Assert.False(Certificate.verifySpkac(spkac));
    }

    private static byte[] CreateSignedPublicKeyAndChallenge(string challenge)
    {
        var keyGenerator = new RsaKeyPairGenerator();
        keyGenerator.Init(new KeyGenerationParameters(new SecureRandom(), 2048));
        var keyPair = keyGenerator.GenerateKeyPair();
        var publicKeyAndChallenge = CreatePublicKeyAndChallenge(keyPair.Public, challenge);
        var signedBytes = publicKeyAndChallenge.GetEncoded();
        var signer = SignerUtilities.GetSigner("SHA256withRSA");
        signer.Init(true, keyPair.Private);
        signer.BlockUpdate(signedBytes, 0, signedBytes.Length);
        var signature = signer.GenerateSignature();
        var signatureAlgorithm = new AlgorithmIdentifier(PkcsObjectIdentifiers.Sha256WithRsaEncryption, DerNull.Instance);
        return new DerSequence(publicKeyAndChallenge, signatureAlgorithm, new DerBitString(signature)).GetEncoded();
    }

    private static Asn1Sequence CreatePublicKeyAndChallenge(AsymmetricKeyParameter publicKey, string challenge)
    {
        var subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKey);
        return new DerSequence(subjectPublicKeyInfo, new DerIA5String(challenge));
    }
}
