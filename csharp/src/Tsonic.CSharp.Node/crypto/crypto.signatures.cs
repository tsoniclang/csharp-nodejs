using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Tsonic.CSharp.Node;

/// <summary>
/// The crypto module provides cryptographic functionality.
/// </summary>
public static partial class crypto
{
    /// <summary>
    /// Generates a symmetric key for the specified algorithm.
    /// </summary>
    public static KeyObject generateKey(string type, object options)
    {
        // Default lengths for common algorithms (in bytes)
        int length = type.ToLowerInvariant() switch
        {
            "aes" or "aes-256-cbc" or "aes-256-gcm" => 32, // 256 bits
            "aes-192-cbc" or "aes-192-gcm" => 24, // 192 bits
            "aes-128-cbc" or "aes-128-gcm" => 16, // 128 bits
            "hmac" => 64, // 512 bits for HMAC
            _ => 32 // Default to 256 bits
        };

        // Generate random bytes
        var keyBytes = new byte[length];
        RandomNumberGenerator.Fill(keyBytes);

        return createSecretKey(keyBytes);
    }

    /// <summary>
    /// Generates a deterministic private key from a password and salt (async).
    /// </summary>
    public static void generateKey(string type, object options, Action<Exception?, KeyObject?> callback)
    {
        try
        {
            var key = generateKey(type, options);
            callback(null, key);
        }
        catch (Exception ex)
        {
            callback(ex, null);
        }
    }

    /// <summary>
    /// Derives a key using the HKDF algorithm.
    /// </summary>
    public static byte[] hkdfSync(string digest, byte[] ikm, byte[] salt, byte[] info, int keylen)
    {
        var hashAlgorithm = GetHashAlgorithmName(digest);
        return HKDF.DeriveKey(hashAlgorithm, ikm, keylen, salt, info);
    }

    /// <summary>
    /// Derives a key using the HKDF algorithm (async).
    /// </summary>
    public static void hkdf(string digest, byte[] ikm, byte[] salt, byte[] info, int keylen, Action<Exception?, byte[]?> callback)
    {
        try
        {
            var key = hkdfSync(digest, ikm, salt, info, keylen);
            callback(null, key);
        }
        catch (Exception ex)
        {
            callback(ex, null);
        }
    }

    /// <summary>
    /// Signs data using a private key.
    /// </summary>
    public static byte[] sign(string? algorithm, byte[] data, string privateKey)
    {
        var sign = createSign(algorithm ?? "sha256");
        sign.update(data);
        return sign.sign(privateKey);
    }

    /// <summary>
    /// Signs data using a private key.
    /// </summary>
    public static byte[] sign(string? algorithm, byte[] data, KeyObject privateKey)
    {
        var sign = createSign(algorithm ?? "sha256");
        sign.update(data);
        return sign.sign(privateKey);
    }

    /// <summary>
    /// Verifies a signature using a public key.
    /// </summary>
    public static bool verify(string? algorithm, byte[] data, string publicKey, byte[] signature)
    {
        var verify = createVerify(algorithm ?? "sha256");
        verify.update(data);
        return verify.verify(publicKey, signature);
    }

    /// <summary>
    /// Verifies a signature using a public key.
    /// </summary>
    public static bool verify(string? algorithm, byte[] data, KeyObject publicKey, byte[] signature)
    {
        var verify = createVerify(algorithm ?? "sha256");
        verify.update(data);
        return verify.verify(publicKey, signature);
    }

    /// <summary>
    /// Computes a hash of the given data.
    /// </summary>
    public static byte[] hash(string algorithm, byte[] data, string? outputEncoding = null)
    {
        var hash = createHash(algorithm);
        hash.update(data);
        return hash.digest();
    }

    /// <summary>
    /// Returns the default cipher list.
    /// </summary>
    public static string getDefaultCipherList()
    {
        return string.Join(":", getCiphers());
    }

    /// <summary>
    /// Gets the Diffie-Hellman group.
    /// </summary>
    public static DiffieHellman getDiffieHellman(string groupName)
    {
        // Predefined DH groups from RFC 3526 and RFC 2409
        if (!MODPGroups.IsValidGroup(groupName))
        {
            throw new ArgumentException($"Unknown DH group: {groupName}");
        }

        var (prime, generator) = MODPGroups.GetGroup(groupName);
        return new DiffieHellman(prime, generator);
    }

    /// <summary>
    /// Creates a DiffieHellman instance with a prime.
    /// </summary>
    public static DiffieHellman createDiffieHellman(byte[] prime, byte[] generator)
    {
        return new DiffieHellman(prime, generator);
    }

    /// <summary>
    /// Creates a DiffieHellman instance with a prime.
    /// </summary>
    public static DiffieHellman createDiffieHellman(byte[] prime, int generator = 2)
    {
        return new DiffieHellman(prime, generator);
    }

    /// <summary>
    /// Creates a DiffieHellman instance with a prime.
    /// </summary>
    public static DiffieHellman createDiffieHellman(string prime, string primeEncoding, int generator = 2)
    {
        var enc = primeEncoding.ToLowerInvariant();
        var primeBytes = enc switch
        {
            "hex" => Convert.FromHexString(prime.Replace("-", "")),
            "base64" => Convert.FromBase64String(prime),
            _ => Encoding.UTF8.GetBytes(prime)
        };
        return new DiffieHellman(primeBytes, generator);
    }

    /// <summary>
    /// Creates a DiffieHellman instance with a prime.
    /// </summary>
    public static DiffieHellman createDiffieHellman(string prime, string primeEncoding, string generator, string generatorEncoding)
    {
        var pEnc = primeEncoding.ToLowerInvariant();
        var gEnc = generatorEncoding.ToLowerInvariant();

        var primeBytes = pEnc switch
        {
            "hex" => Convert.FromHexString(prime.Replace("-", "")),
            "base64" => Convert.FromBase64String(prime),
            _ => Encoding.UTF8.GetBytes(prime)
        };

        var generatorBytes = gEnc switch
        {
            "hex" => Convert.FromHexString(generator.Replace("-", "")),
            "base64" => Convert.FromBase64String(generator),
            _ => Encoding.UTF8.GetBytes(generator)
        };

        return new DiffieHellman(primeBytes, generatorBytes);
    }

    /// <summary>
    /// Sets the default encoding for crypto operations.
    /// </summary>
    public static void setDefaultEncoding(string encoding)
    {
        // This is a legacy Node.js API that's deprecated
        // We'll just ignore it for now
    }

    /// <summary>
    /// Gets the fips mode status (always false in .NET).
    /// </summary>
    public static bool getFips()
    {
        // .NET uses CryptoConfig but doesn't have a FIPS mode flag
        return false;
    }

    /// <summary>
    /// Sets the fips mode (not supported in .NET).
    /// </summary>
    public static void setFips(bool enabled)
    {
        if (enabled)
        {
            throw new NotSupportedException("FIPS mode is not directly configurable in .NET. Use system-level FIPS policy instead.");
        }
    }

    private static HashAlgorithmName GetHashAlgorithmName(string digest)
    {
        return digest.ToLowerInvariant() switch
        {
            "sha1" => HashAlgorithmName.SHA1,
            "sha256" => HashAlgorithmName.SHA256,
            "sha384" => HashAlgorithmName.SHA384,
            "sha512" => HashAlgorithmName.SHA512,
            "md5" => HashAlgorithmName.MD5,
            _ => throw new ArgumentException($"Unsupported digest algorithm: {digest}")
        };
    }
}
