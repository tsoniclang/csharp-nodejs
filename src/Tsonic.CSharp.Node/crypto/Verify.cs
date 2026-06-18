using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.OpenSsl;
using System.IO;
using Org.BouncyCastle.Math;

namespace Tsonic.CSharp.Node;

/// <summary>
/// The Verify class is a utility for verifying signatures.
/// </summary>
public class Verify : Transform
{
    private readonly string _algorithm;
    private readonly MemoryStream _dataStream;
    private bool _finalized = false;

    internal Verify(string algorithm)
    {
        _algorithm = algorithm;
        _dataStream = new MemoryStream();
    }

    /// <summary>
    /// Updates the Verify content with the given data.
    /// </summary>
    /// <param name="data">The data to verify.</param>
    /// <param name="inputEncoding">The encoding of the data string.</param>
    /// <returns>The Verify object for chaining.</returns>
    public Verify update(string data, string? inputEncoding = null)
    {
        if (_finalized)
            throw new InvalidOperationException("Verify already finalized");

        var encoding = GetEncoding(inputEncoding ?? "utf8");
        var bytes = encoding.GetBytes(data);
        return update(bytes);
    }

    /// <summary>
    /// Updates the Verify content with the given data.
    /// </summary>
    /// <param name="data">The data to verify.</param>
    /// <returns>The Verify object for chaining.</returns>
    public Verify update(byte[] data)
    {
        if (_finalized)
            throw new InvalidOperationException("Verify already finalized");

        _dataStream.Write(data, 0, data.Length);
        return this;
    }

    /// <summary>
    /// Verifies the provided data using the given public key and signature.
    /// </summary>
    /// <param name="publicKey">The public key for verification.</param>
    /// <param name="signature">The signature to verify.</param>
    /// <param name="signatureEncoding">The encoding of the signature.</param>
    /// <returns>True if verification succeeds, false otherwise.</returns>
    public bool verify(string publicKey, string signature, string? signatureEncoding = null)
    {
        byte[] signatureBytes;
        var encoding = (signatureEncoding ?? "base64").ToLowerInvariant();

        signatureBytes = encoding switch
        {
            "hex" => Convert.FromHexString(signature.Replace("-", "")),
            "base64" => Convert.FromBase64String(signature),
            "base64url" => Convert.FromBase64String(signature.Replace("-", "+").Replace("_", "/")),
            "latin1" or "binary" => Encoding.Latin1.GetBytes(signature),
            _ => Encoding.UTF8.GetBytes(signature)
        };

        return verify(publicKey, signatureBytes);
    }

    /// <summary>
    /// Verifies the provided data using the given public key and signature.
    /// </summary>
    /// <param name="publicKey">The public key for verification.</param>
    /// <param name="signature">The signature to verify.</param>
    /// <returns>True if verification succeeds, false otherwise.</returns>
    public bool verify(string publicKey, byte[] signature)
    {
        if (_finalized)
            throw new InvalidOperationException("Verify already finalized");

        _finalized = true;
        var data = _dataStream.ToArray();

        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKey);

            var hashAlgorithm = GetHashAlgorithmName(_algorithm);
            return rsa.VerifyData(data, signature, hashAlgorithm, RSASignaturePadding.Pkcs1);
        }
        catch (Exception)
        {
            // Try other key formats
            try
            {
                using var ecdsa = ECDsa.Create();
                ecdsa.ImportFromPem(publicKey);

                var hashAlgorithm = GetHashAlgorithmName(_algorithm);
                return ecdsa.VerifyData(data, signature, hashAlgorithm);
            }
            catch (Exception)
            {
                // Try DSA using BouncyCastle
                try
                {
                    using var reader = new StringReader(publicKey);
                    var pemReader = new PemReader(reader);
                    var keyObject = pemReader.ReadObject();

                    AsymmetricKeyParameter dsaKey = keyObject switch
                    {
                        AsymmetricCipherKeyPair keyPair => keyPair.Public,
                        AsymmetricKeyParameter key => key,
                        _ => throw new ArgumentException("Invalid DSA key format")
                    };

                    return VerifyWithDsa(data, signature, dsaKey, _algorithm);
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }

    private bool VerifyWithDsa(byte[] data, byte[] signature, AsymmetricKeyParameter publicKey, string algorithm)
    {
        try
        {
            // Hash the data first
            var digest = CreateBouncyCastleDigest(algorithm);
            digest.BlockUpdate(data, 0, data.Length);
            var hash = new byte[digest.GetDigestSize()];
            digest.DoFinal(hash, 0);

            // Parse DER encoded signature to extract r and s
            var (r, s) = ParseDerSignature(signature);

            // Verify the signature
            var signer = new DsaSigner();
            signer.Init(false, publicKey);
            return signer.VerifySignature(hash, r, s);
        }
        catch
        {
            return false;
        }
    }

    private (BigInteger r, BigInteger s) ParseDerSignature(byte[] signature)
    {
        using var ms = new MemoryStream(signature);
        using var reader = new BinaryReader(ms);

        // Read SEQUENCE tag
        if (reader.ReadByte() != 0x30)
            throw new ArgumentException("Invalid DER signature format");

        // Read sequence length
        reader.ReadByte();

        // Read r
        if (reader.ReadByte() != 0x02)
            throw new ArgumentException("Invalid DER signature format - r tag");
        int rLen = reader.ReadByte();
        var rBytes = reader.ReadBytes(rLen);
        var r = new BigInteger(1, rBytes);

        // Read s
        if (reader.ReadByte() != 0x02)
            throw new ArgumentException("Invalid DER signature format - s tag");
        int sLen = reader.ReadByte();
        var sBytes = reader.ReadBytes(sLen);
        var s = new BigInteger(1, sBytes);

        return (r, s);
    }

    private IDigest CreateBouncyCastleDigest(string algorithm)
    {
        return algorithm.ToLowerInvariant() switch
        {
            "sha1" => new Sha1Digest(),
            "sha256" => new Sha256Digest(),
            "sha384" => new Sha384Digest(),
            "sha512" => new Sha512Digest(),
            "sha3-256" => new Sha3Digest(256),
            "sha3-384" => new Sha3Digest(384),
            "sha3-512" => new Sha3Digest(512),
            _ => new Sha256Digest() // Default to SHA256
        };
    }

    /// <summary>
    /// Verifies the provided data using the given public key object and signature.
    /// </summary>
    /// <param name="publicKey">The public key object for verification.</param>
    /// <param name="signature">The signature to verify.</param>
    /// <param name="signatureEncoding">The encoding of the signature.</param>
    /// <returns>True if verification succeeds, false otherwise.</returns>
    public bool verify(object publicKey, string signature, string? signatureEncoding = null)
    {
        byte[] signatureBytes;
        var encoding = (signatureEncoding ?? "base64").ToLowerInvariant();

        signatureBytes = encoding switch
        {
            "hex" => Convert.FromHexString(signature.Replace("-", "")),
            "base64" => Convert.FromBase64String(signature),
            "base64url" => Convert.FromBase64String(signature.Replace("-", "+").Replace("_", "/")),
            "latin1" or "binary" => Encoding.Latin1.GetBytes(signature),
            _ => Encoding.UTF8.GetBytes(signature)
        };

        return verify(publicKey, signatureBytes);
    }

    /// <summary>
    /// Verifies the provided data using the given public key object and signature.
    /// </summary>
    /// <param name="publicKey">The public key object for verification.</param>
    /// <param name="signature">The signature to verify.</param>
    /// <returns>True if verification succeeds, false otherwise.</returns>
    public bool verify(object publicKey, byte[] signature)
    {
        if (_finalized)
            throw new InvalidOperationException("Verify already finalized");

        _finalized = true;
        var data = _dataStream.ToArray();

        // Check for DSA key first
        if (publicKey is DSAPublicKeyObject dsaPublicKey)
        {
            var dsaKey = dsaPublicKey.GetKey();
            return VerifyWithDsa(data, signature, dsaKey, _algorithm);
        }

        if (publicKey is not PublicKeyObject keyObject)
            throw new ArgumentException("publicKey must be a PublicKeyObject", nameof(publicKey));

        var key = keyObject.GetKey();
        var hashAlgorithm = GetHashAlgorithmName(_algorithm);

        if (key is RSA rsa)
        {
            return rsa.VerifyData(data, signature, hashAlgorithm, RSASignaturePadding.Pkcs1);
        }
        else if (key is ECDsa ecdsa)
        {
            return ecdsa.VerifyData(data, signature, hashAlgorithm);
        }
        else
        {
            throw new NotSupportedException($"Verification with key type {keyObject.asymmetricKeyType} is not supported");
        }
    }

#pragma warning disable CS1591
    ~Verify()
    {
        Dispose(false);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            _dataStream?.Dispose();
        }
    }
#pragma warning restore CS1591

    private static HashAlgorithmName GetHashAlgorithmName(string algorithm)
    {
        var alg = algorithm.ToLowerInvariant();

        // Handle algorithm names with and without dashes
        alg = alg.Replace("sha-", "sha").Replace("rsa-", "");

        return alg switch
        {
            "sha1" => HashAlgorithmName.SHA1,
            "sha256" => HashAlgorithmName.SHA256,
            "sha384" => HashAlgorithmName.SHA384,
            "sha512" => HashAlgorithmName.SHA512,
            "md5" => HashAlgorithmName.MD5,
            _ when alg.Contains("sha1") => HashAlgorithmName.SHA1,
            _ when alg.Contains("sha256") => HashAlgorithmName.SHA256,
            _ when alg.Contains("sha384") => HashAlgorithmName.SHA384,
            _ when alg.Contains("sha512") => HashAlgorithmName.SHA512,
            _ => throw new ArgumentException($"Unsupported signature algorithm: {algorithm}")
        };
    }

    private static Encoding GetEncoding(string encoding)
    {
        return encoding.ToLowerInvariant() switch
        {
            "utf8" or "utf-8" => Encoding.UTF8,
            "ascii" => Encoding.ASCII,
            "latin1" or "binary" => Encoding.Latin1,
            "utf16le" or "utf-16le" => Encoding.Unicode,
            "base64" => Encoding.ASCII,
            "hex" => Encoding.ASCII,
            _ => Encoding.UTF8
        };
    }
}
