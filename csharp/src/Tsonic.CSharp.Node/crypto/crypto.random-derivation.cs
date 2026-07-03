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
    /// Generates a UUID v4.
    /// </summary>
    /// <returns>A UUID string.</returns>
    public static string randomUUID()
    {
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Provides an asynchronous Password-Based Key Derivation Function 2 (PBKDF2) implementation.
    /// </summary>
    public static byte[] pbkdf2Sync(string password, string salt, int iterations, int keylen, string digest)
    {
        return pbkdf2Sync(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt), iterations, keylen, digest);
    }

    /// <summary>
    /// Provides an asynchronous Password-Based Key Derivation Function 2 (PBKDF2) implementation.
    /// </summary>
    public static byte[] pbkdf2Sync(byte[] password, byte[] salt, int iterations, int keylen, string digest)
    {
        var hashAlgorithm = GetHashAlgorithmName(digest);
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, hashAlgorithm, keylen);
    }

    /// <summary>
    /// Provides an asynchronous Password-Based Key Derivation Function 2 (PBKDF2) implementation.
    /// </summary>
    public static void pbkdf2(string password, string salt, int iterations, int keylen, string digest, Action<Exception?, byte[]?> callback)
    {
        try
        {
            var result = pbkdf2Sync(password, salt, iterations, keylen, digest);
            callback(null, result);
        }
        catch (Exception ex)
        {
            callback(ex, null);
        }
    }

    /// <summary>
    /// Provides a synchronous scrypt implementation.
    /// </summary>
    public static byte[] scryptSync(string password, string salt, int keylen, object? options = null)
    {
        return scryptSync(Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(salt), keylen, options);
    }

    /// <summary>
    /// Provides a synchronous scrypt implementation.
    /// </summary>
    public static byte[] scryptSync(byte[] password, byte[] salt, int keylen, object? options = null)
    {
        // Default scrypt parameters
        int N = 16384;  // CPU/memory cost parameter (must be power of 2)
        int r = 8;      // Block size parameter
        int p = 1;      // Parallelization parameter

        // Parse options if provided
        if (options != null)
        {
            var optionsDict = options as System.Collections.Generic.Dictionary<string, object>;
            if (optionsDict != null)
            {
                if (optionsDict.TryGetValue("N", out var nValue))
                    N = Convert.ToInt32(nValue);
                if (optionsDict.TryGetValue("cost", out var costValue))
                    N = Convert.ToInt32(costValue);
                if (optionsDict.TryGetValue("r", out var rValue))
                    r = Convert.ToInt32(rValue);
                if (optionsDict.TryGetValue("blockSize", out var blockSizeValue))
                    r = Convert.ToInt32(blockSizeValue);
                if (optionsDict.TryGetValue("p", out var pValue))
                    p = Convert.ToInt32(pValue);
                if (optionsDict.TryGetValue("parallelization", out var parallelizationValue))
                    p = Convert.ToInt32(parallelizationValue);
            }
        }

        return SCrypt.Generate(password, salt, N, r, p, keylen);
    }

    /// <summary>
    /// Provides an asynchronous scrypt implementation.
    /// </summary>
    public static void scrypt(string password, string salt, int keylen, object? options, Action<Exception?, byte[]?> callback)
    {
        try
        {
            var result = scryptSync(password, salt, keylen, options);
            callback(null, result);
        }
        catch (Exception ex)
        {
            callback(ex, null);
        }
    }

    /// <summary>
    /// Returns an array of the names of the supported cipher algorithms.
    /// </summary>
    /// <returns>An array of cipher algorithm names.</returns>
    public static string[] getCiphers()
    {
        return new[]
        {
            "aes-128-cbc", "aes-128-ecb", "aes-128-cfb",
            "aes-192-cbc", "aes-192-ecb", "aes-192-cfb",
            "aes-256-cbc", "aes-256-ecb", "aes-256-cfb",
            "des-cbc", "des-ecb",
            "des-ede3-cbc", "des-ede3-ecb",
            "rc2-cbc", "rc2-ecb"
        };
    }

    /// <summary>
    /// Returns an array of the names of the supported hash algorithms.
    /// </summary>
    /// <returns>An array of hash algorithm names.</returns>
    public static string[] getHashes()
    {
        return new[]
        {
            "md5",
            "sha1", "sha256", "sha384", "sha512"
        };
    }

    /// <summary>
    /// Returns an array of the names of the supported elliptic curves.
    /// </summary>
    /// <returns>An array of curve names.</returns>
    public static string[] getCurves()
    {
        return new[]
        {
            "secp256r1", "secp384r1", "secp521r1",
            "secp256k1",
            "ed25519", "ed448",
            "x25519", "x448"
        };
    }

    /// <summary>
    /// Test for equality in constant time.
    /// </summary>
    /// <param name="a">First buffer.</param>
    /// <param name="b">Second buffer.</param>
    /// <returns>True if the buffers are equal.</returns>
    public static bool timingSafeEqual(byte[] a, byte[] b)
    {
        if (a.Length != b.Length)
            return false;

        int result = 0;
        for (int i = 0; i < a.Length; i++)
        {
            result |= a[i] ^ b[i];
        }
        return result == 0;
    }

    /// <summary>
    /// Test for Buffer equality in constant time.
    /// </summary>
    /// <param name="a">First Buffer.</param>
    /// <param name="b">Second Buffer.</param>
    /// <returns>True if the buffers are equal.</returns>
    public static bool timingSafeEqual(Buffer a, Buffer b)
    {
        if (a == null)
            throw new ArgumentNullException(nameof(a));
        if (b == null)
            throw new ArgumentNullException(nameof(b));

        return timingSafeEqual(a.InternalData, b.InternalData);
    }

}
