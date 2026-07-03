using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Math;

namespace Tsonic.CSharp.Node;

/// <summary>
/// The DiffieHellman class is a utility for creating Diffie-Hellman key exchanges.
/// </summary>
public class DiffieHellman : IDisposable
{
    private byte[] _prime;
    private byte[] _generator;
    private byte[]? _privateKey;
    private byte[]? _publicKey;
    private bool _disposed = false;

    internal DiffieHellman(int primeLength, int generator = 2)
    {
        // Generate DH parameters using BouncyCastle
        var dhGen = new DHParametersGenerator();
        dhGen.Init(primeLength, 128, new SecureRandom()); // 128-bit certainty for primality test
        var dhParams = dhGen.GenerateParameters();

        // Convert BouncyCastle BigInteger to byte array
        _prime = dhParams.P.ToByteArrayUnsigned();
        _generator = BigInteger.ValueOf(generator).ToByteArrayUnsigned();
    }

    internal DiffieHellman(byte[] prime, byte[] generator)
    {
        _prime = prime ?? throw new ArgumentNullException(nameof(prime));
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
    }

    internal DiffieHellman(byte[] prime, int generator)
    {
        _prime = prime ?? throw new ArgumentNullException(nameof(prime));
        _generator = BitConverter.GetBytes(generator);
    }

    /// <summary>
    /// Generates private and public Diffie-Hellman key values.
    /// </summary>
    /// <param name="encoding">The encoding of the return value.</param>
    /// <returns>The public key.</returns>
    public string generateKeys(string? encoding = null)
    {
        var publicKey = generateKeys();

        if (encoding == null || encoding == "buffer")
        {
            return Convert.ToBase64String(publicKey);
        }

        return encoding.ToLowerInvariant() switch
        {
            "hex" => BitConverter.ToString(publicKey).Replace("-", "").ToLowerInvariant(),
            "base64" => Convert.ToBase64String(publicKey),
            "base64url" => Convert.ToBase64String(publicKey).Replace("+", "-").Replace("/", "_").TrimEnd('='),
            "latin1" or "binary" => System.Text.Encoding.Latin1.GetString(publicKey),
            _ => throw new ArgumentException($"Unknown encoding: {encoding}")
        };
    }

    /// <summary>
    /// Generates private and public Diffie-Hellman key values.
    /// </summary>
    /// <returns>The public key as a byte array.</returns>
    public byte[] generateKeys()
    {
        // Generate random private key
        _privateKey = new byte[_prime.Length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(_privateKey);
        }

        // Ensure private key is less than prime
        EnsureLessThan(_privateKey, _prime);

        // Calculate public key: g^privateKey mod p
        _publicKey = ModPow(_generator, _privateKey, _prime);

        return _publicKey;
    }

    /// <summary>
    /// Computes the shared secret using the other party's public key.
    /// </summary>
    /// <param name="otherPublicKey">The other party's public key.</param>
    /// <param name="inputEncoding">The encoding of the input key.</param>
    /// <param name="outputEncoding">The encoding of the return value.</param>
    /// <returns>The shared secret.</returns>
    public string computeSecret(string otherPublicKey, string? inputEncoding = null, string? outputEncoding = null)
    {
        byte[] otherKeyBytes;
        var inEnc = (inputEncoding ?? "base64").ToLowerInvariant();

        otherKeyBytes = inEnc switch
        {
            "hex" => Convert.FromHexString(otherPublicKey.Replace("-", "")),
            "base64" => Convert.FromBase64String(otherPublicKey),
            "base64url" => Convert.FromBase64String(otherPublicKey.Replace("-", "+").Replace("_", "/")),
            "latin1" or "binary" => System.Text.Encoding.Latin1.GetBytes(otherPublicKey),
            _ => System.Text.Encoding.UTF8.GetBytes(otherPublicKey)
        };

        return computeSecret(otherKeyBytes, outputEncoding);
    }

    /// <summary>
    /// Computes the shared secret using the other party's public key.
    /// </summary>
    /// <param name="otherPublicKey">The other party's public key.</param>
    /// <param name="outputEncoding">The encoding of the return value.</param>
    /// <returns>The shared secret.</returns>
    public string computeSecret(byte[] otherPublicKey, string? outputEncoding = null)
    {
        var secret = computeSecret(otherPublicKey);

        if (outputEncoding == null || outputEncoding == "buffer")
        {
            return Convert.ToBase64String(secret);
        }

        return outputEncoding.ToLowerInvariant() switch
        {
            "hex" => BitConverter.ToString(secret).Replace("-", "").ToLowerInvariant(),
            "base64" => Convert.ToBase64String(secret),
            "base64url" => Convert.ToBase64String(secret).Replace("+", "-").Replace("/", "_").TrimEnd('='),
            "latin1" or "binary" => System.Text.Encoding.Latin1.GetString(secret),
            _ => throw new ArgumentException($"Unknown encoding: {outputEncoding}")
        };
    }

    /// <summary>
    /// Computes the shared secret using the other party's public key.
    /// </summary>
    /// <param name="otherPublicKey">The other party's public key.</param>
    /// <returns>The shared secret as a byte array.</returns>
    public byte[] computeSecret(byte[] otherPublicKey)
    {
        if (_privateKey == null)
            throw new InvalidOperationException("Must call generateKeys() first");

        // Calculate shared secret: otherPublicKey^privateKey mod p
        return ModPow(otherPublicKey, _privateKey, _prime);
    }

    /// <summary>
    /// Returns the Diffie-Hellman prime.
    /// </summary>
    /// <param name="encoding">The encoding of the return value.</param>
    /// <returns>The prime.</returns>
    public string getPrime(string? encoding = null)
    {
        var prime = getPrime();

        if (encoding == null || encoding == "buffer")
        {
            return Convert.ToBase64String(prime);
        }

        return encoding.ToLowerInvariant() switch
        {
            "hex" => BitConverter.ToString(prime).Replace("-", "").ToLowerInvariant(),
            "base64" => Convert.ToBase64String(prime),
            "base64url" => Convert.ToBase64String(prime).Replace("+", "-").Replace("/", "_").TrimEnd('='),
            "latin1" or "binary" => System.Text.Encoding.Latin1.GetString(prime),
            _ => throw new ArgumentException($"Unknown encoding: {encoding}")
        };
    }

    /// <summary>
    /// Returns the Diffie-Hellman prime as a byte array.
    /// </summary>
    /// <returns>The prime.</returns>
    public byte[] getPrime()
    {
        return _prime;
    }

    /// <summary>
    /// Returns the Diffie-Hellman generator.
    /// </summary>
    /// <param name="encoding">The encoding of the return value.</param>
    /// <returns>The generator.</returns>
    public string getGenerator(string? encoding = null)
    {
        var generator = getGenerator();

        if (encoding == null || encoding == "buffer")
        {
            return Convert.ToBase64String(generator);
        }

        return encoding.ToLowerInvariant() switch
        {
            "hex" => BitConverter.ToString(generator).Replace("-", "").ToLowerInvariant(),
            "base64" => Convert.ToBase64String(generator),
            "base64url" => Convert.ToBase64String(generator).Replace("+", "-").Replace("/", "_").TrimEnd('='),
            "latin1" or "binary" => System.Text.Encoding.Latin1.GetString(generator),
            _ => throw new ArgumentException($"Unknown encoding: {encoding}")
        };
    }

    /// <summary>
    /// Returns the Diffie-Hellman generator as a byte array.
    /// </summary>
    /// <returns>The generator.</returns>
    public byte[] getGenerator()
    {
        return _generator;
    }

    /// <summary>
    /// Returns the Diffie-Hellman public key.
    /// </summary>
    /// <param name="encoding">The encoding of the return value.</param>
    /// <returns>The public key.</returns>
    public string getPublicKey(string? encoding = null)
    {
        var publicKey = getPublicKey();

        if (encoding == null || encoding == "buffer")
        {
            return Convert.ToBase64String(publicKey);
        }

        return encoding.ToLowerInvariant() switch
        {
            "hex" => BitConverter.ToString(publicKey).Replace("-", "").ToLowerInvariant(),
            "base64" => Convert.ToBase64String(publicKey),
            "base64url" => Convert.ToBase64String(publicKey).Replace("+", "-").Replace("/", "_").TrimEnd('='),
            "latin1" or "binary" => System.Text.Encoding.Latin1.GetString(publicKey),
            _ => throw new ArgumentException($"Unknown encoding: {encoding}")
        };
    }

    /// <summary>
    /// Returns the Diffie-Hellman public key as a byte array.
    /// </summary>
    /// <returns>The public key.</returns>
    public byte[] getPublicKey()
    {
        if (_publicKey == null)
            throw new InvalidOperationException("Must call generateKeys() first");

        return _publicKey;
    }

    /// <summary>
    /// Returns the Diffie-Hellman private key.
    /// </summary>
    /// <param name="encoding">The encoding of the return value.</param>
    /// <returns>The private key.</returns>
    public string getPrivateKey(string? encoding = null)
    {
        var privateKey = getPrivateKey();

        if (encoding == null || encoding == "buffer")
        {
            return Convert.ToBase64String(privateKey);
        }

        return encoding.ToLowerInvariant() switch
        {
            "hex" => BitConverter.ToString(privateKey).Replace("-", "").ToLowerInvariant(),
            "base64" => Convert.ToBase64String(privateKey),
            "base64url" => Convert.ToBase64String(privateKey).Replace("+", "-").Replace("/", "_").TrimEnd('='),
            "latin1" or "binary" => System.Text.Encoding.Latin1.GetString(privateKey),
            _ => throw new ArgumentException($"Unknown encoding: {encoding}")
        };
    }

    /// <summary>
    /// Returns the Diffie-Hellman private key as a byte array.
    /// </summary>
    /// <returns>The private key.</returns>
    public byte[] getPrivateKey()
    {
        if (_privateKey == null)
            throw new InvalidOperationException("Must call generateKeys() first");

        return _privateKey;
    }

    /// <summary>
    /// Sets the Diffie-Hellman public key.
    /// </summary>
    /// <param name="publicKey">The public key.</param>
    /// <param name="encoding">The encoding of the public key.</param>
    public void setPublicKey(string publicKey, string? encoding = null)
    {
        var enc = (encoding ?? "base64").ToLowerInvariant();

        var keyBytes = enc switch
        {
            "hex" => Convert.FromHexString(publicKey.Replace("-", "")),
            "base64" => Convert.FromBase64String(publicKey),
            "base64url" => Convert.FromBase64String(publicKey.Replace("-", "+").Replace("_", "/")),
            "latin1" or "binary" => System.Text.Encoding.Latin1.GetBytes(publicKey),
            _ => System.Text.Encoding.UTF8.GetBytes(publicKey)
        };

        setPublicKey(keyBytes);
    }

    /// <summary>
    /// Sets the Diffie-Hellman public key.
    /// </summary>
    /// <param name="publicKey">The public key.</param>
    public void setPublicKey(byte[] publicKey)
    {
        _publicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
    }

    /// <summary>
    /// Sets the Diffie-Hellman private key.
    /// </summary>
    /// <param name="privateKey">The private key.</param>
    /// <param name="encoding">The encoding of the private key.</param>
    public void setPrivateKey(string privateKey, string? encoding = null)
    {
        var enc = (encoding ?? "base64").ToLowerInvariant();

        var keyBytes = enc switch
        {
            "hex" => Convert.FromHexString(privateKey.Replace("-", "")),
            "base64" => Convert.FromBase64String(privateKey),
            "base64url" => Convert.FromBase64String(privateKey.Replace("-", "+").Replace("_", "/")),
            "latin1" or "binary" => System.Text.Encoding.Latin1.GetBytes(privateKey),
            _ => System.Text.Encoding.UTF8.GetBytes(privateKey)
        };

        setPrivateKey(keyBytes);
    }

    /// <summary>
    /// Sets the Diffie-Hellman private key.
    /// </summary>
    /// <param name="privateKey">The private key.</param>
    public void setPrivateKey(byte[] privateKey)
    {
        _privateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));

        // Recalculate public key
        if (_privateKey != null)
        {
            _publicKey = ModPow(_generator, _privateKey, _prime);
        }
    }

    /// <summary>
    /// Returns the size of the Diffie-Hellman group in bits.
    /// </summary>
    /// <returns>The size in bits.</returns>
    public int getVerifyError()
    {
        // In Node.js, this returns DH validation warnings/errors
        // For now, we assume the DH parameters are valid
        return 0;
    }

#pragma warning disable CS1591
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // Clear sensitive data
                if (_privateKey != null)
                {
                    Array.Clear(_privateKey, 0, _privateKey.Length);
                }
                if (_publicKey != null)
                {
                    Array.Clear(_publicKey, 0, _publicKey.Length);
                }
            }

            _disposed = true;
        }
    }
#pragma warning restore CS1591

    // Helper method for modular exponentiation: (base^exponent) mod modulus
    private static byte[] ModPow(byte[] baseBytes, byte[] exponentBytes, byte[] modulusBytes)
    {
        var baseNum = new System.Numerics.BigInteger(baseBytes, isUnsigned: true, isBigEndian: false);
        var exponent = new System.Numerics.BigInteger(exponentBytes, isUnsigned: true, isBigEndian: false);
        var modulus = new System.Numerics.BigInteger(modulusBytes, isUnsigned: true, isBigEndian: false);

        var result = System.Numerics.BigInteger.ModPow(baseNum, exponent, modulus);
        return result.ToByteArray(isUnsigned: true, isBigEndian: false);
    }

    // Helper method to ensure a number is less than another
    private static void EnsureLessThan(byte[] value, byte[] max)
    {
        var valueNum = new System.Numerics.BigInteger(value, isUnsigned: true, isBigEndian: false);
        var maxNum = new System.Numerics.BigInteger(max, isUnsigned: true, isBigEndian: false);

        if (valueNum >= maxNum)
        {
            // Reduce value to be within range
            valueNum = valueNum % maxNum;
            var newBytes = valueNum.ToByteArray(isUnsigned: true, isBigEndian: false);
            Array.Clear(value, 0, value.Length);
            Array.Copy(newBytes, value, Math.Min(newBytes.Length, value.Length));
        }
    }
}
