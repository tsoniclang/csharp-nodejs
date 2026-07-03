using System;
using System.Security.Cryptography;
using System.Text;

namespace Tsonic.CSharp.Node;

/// <summary>
/// The ECDH class is a utility for creating Elliptic Curve Diffie-Hellman (ECDH) key exchanges.
/// </summary>
public class ECDH : IDisposable
{
    private readonly ECDiffieHellman _ecdh;
    private readonly string _curveName;
    private bool _disposed = false;

    internal ECDH(string curveName)
    {
        _curveName = curveName ?? throw new ArgumentNullException(nameof(curveName));

        var curve = GetECCurve(curveName);
        _ecdh = ECDiffieHellman.Create(curve);
    }

    /// <summary>
    /// Generates private and public EC Diffie-Hellman key values.
    /// </summary>
    /// <param name="encoding">The encoding of the return value.</param>
    /// <param name="format">The format of the public key (compressed, uncompressed, or hybrid).</param>
    /// <returns>The public key.</returns>
    public string generateKeys(string? encoding = null, string? format = null)
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
            "latin1" or "binary" => Encoding.Latin1.GetString(publicKey),
            _ => throw new ArgumentException($"Unknown encoding: {encoding}")
        };
    }

    /// <summary>
    /// Generates private and public EC Diffie-Hellman key values.
    /// </summary>
    /// <returns>The public key as a byte array.</returns>
    public byte[] generateKeys()
    {
        // Keys are already generated in the constructor
        // Just export the public key
        return _ecdh.PublicKey.ExportSubjectPublicKeyInfo();
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
            "latin1" or "binary" => Encoding.Latin1.GetBytes(otherPublicKey),
            _ => Encoding.UTF8.GetBytes(otherPublicKey)
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
            "latin1" or "binary" => Encoding.Latin1.GetString(secret),
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
        try
        {
            // Try to import as SubjectPublicKeyInfo (standard format)
            using var otherEcdh = ECDiffieHellman.Create();
            otherEcdh.ImportSubjectPublicKeyInfo(otherPublicKey, out _);
            return _ecdh.DeriveKeyMaterial(otherEcdh.PublicKey);
        }
        catch
        {
            // Try raw public key format
            try
            {
                var curve = GetECCurve(_curveName);
                using var otherEcdh = ECDiffieHellman.Create(curve);

                // Import ECPoint (raw format)
                var parameters = otherEcdh.ExportParameters(false);

                // Parse the raw public key (assuming uncompressed format: 0x04 || X || Y)
                if (otherPublicKey.Length > 0 && otherPublicKey[0] == 0x04)
                {
                    var coordinateSize = (otherPublicKey.Length - 1) / 2;
                    parameters.Q.X = new byte[coordinateSize];
                    parameters.Q.Y = new byte[coordinateSize];
                    Array.Copy(otherPublicKey, 1, parameters.Q.X, 0, coordinateSize);
                    Array.Copy(otherPublicKey, 1 + coordinateSize, parameters.Q.Y, 0, coordinateSize);
                }
                else
                {
                    throw new ArgumentException("Unsupported public key format");
                }

                otherEcdh.ImportParameters(parameters);
                return _ecdh.DeriveKeyMaterial(otherEcdh.PublicKey);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Failed to import other party's public key", ex);
            }
        }
    }

    /// <summary>
    /// Returns the EC Diffie-Hellman public key.
    /// </summary>
    /// <param name="encoding">The encoding of the return value.</param>
    /// <param name="format">The format of the public key (compressed, uncompressed, or hybrid).</param>
    /// <returns>The public key.</returns>
    public string getPublicKey(string? encoding = null, string? format = null)
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
            "latin1" or "binary" => Encoding.Latin1.GetString(publicKey),
            _ => throw new ArgumentException($"Unknown encoding: {encoding}")
        };
    }

    /// <summary>
    /// Returns the EC Diffie-Hellman public key as a byte array.
    /// </summary>
    /// <returns>The public key.</returns>
    public byte[] getPublicKey()
    {
        return _ecdh.PublicKey.ExportSubjectPublicKeyInfo();
    }

    /// <summary>
    /// Returns the EC Diffie-Hellman private key.
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
            "latin1" or "binary" => Encoding.Latin1.GetString(privateKey),
            _ => throw new ArgumentException($"Unknown encoding: {encoding}")
        };
    }

    /// <summary>
    /// Returns the EC Diffie-Hellman private key as a byte array.
    /// </summary>
    /// <returns>The private key.</returns>
    public byte[] getPrivateKey()
    {
        return _ecdh.ExportECPrivateKey();
    }

    /// <summary>
    /// Sets the EC Diffie-Hellman public key.
    /// </summary>
    /// <param name="publicKey">The public key.</param>
    /// <param name="encoding">The encoding of the public key.</param>
    public void setPublicKey(string publicKey, string? encoding = null)
    {
        // Note: In Node.js, setPublicKey is deprecated and throws an error
        throw new NotSupportedException("setPublicKey() is not supported. Use setPrivateKey() instead.");
    }

    /// <summary>
    /// Sets the EC Diffie-Hellman public key.
    /// </summary>
    /// <param name="publicKey">The public key.</param>
    public void setPublicKey(byte[] publicKey)
    {
        // Note: In Node.js, setPublicKey is deprecated and throws an error
        throw new NotSupportedException("setPublicKey() is not supported. Use setPrivateKey() instead.");
    }

    /// <summary>
    /// Sets the EC Diffie-Hellman private key.
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
            "latin1" or "binary" => Encoding.Latin1.GetBytes(privateKey),
            _ => Encoding.UTF8.GetBytes(privateKey)
        };

        setPrivateKey(keyBytes);
    }

    /// <summary>
    /// Sets the EC Diffie-Hellman private key.
    /// </summary>
    /// <param name="privateKey">The private key.</param>
    public void setPrivateKey(byte[] privateKey)
    {
        _ecdh.ImportECPrivateKey(privateKey, out _);
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
                _ecdh?.Dispose();
            }

            _disposed = true;
        }
    }
#pragma warning restore CS1591

    private static ECCurve GetECCurve(string curveName)
    {
        return curveName.ToLowerInvariant() switch
        {
            "secp256r1" or "prime256v1" or "p-256" => ECCurve.NamedCurves.nistP256,
            "secp384r1" or "p-384" => ECCurve.NamedCurves.nistP384,
            "secp521r1" or "p-521" => ECCurve.NamedCurves.nistP521,
            "secp256k1" => CreateSecp256k1Curve(),
            "brainpoolp256r1" => ECCurve.NamedCurves.brainpoolP256r1,
            "brainpoolp384r1" => ECCurve.NamedCurves.brainpoolP384r1,
            "brainpoolp512r1" => ECCurve.NamedCurves.brainpoolP512r1,
            _ => throw new ArgumentException($"Unknown or unsupported curve: {curveName}")
        };
    }

    private static ECCurve CreateSecp256k1Curve()
    {
        // secp256k1 curve parameters (used by Bitcoin)
        return new ECCurve
        {
            CurveType = ECCurve.ECCurveType.PrimeShortWeierstrass,
            Prime = Convert.FromHexString("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F"),
            A = Convert.FromHexString("0000000000000000000000000000000000000000000000000000000000000000"),
            B = Convert.FromHexString("0000000000000000000000000000000000000000000000000000000000000007"),
            G = new ECPoint
            {
                X = Convert.FromHexString("79BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798"),
                Y = Convert.FromHexString("483ADA7726A3C4655DA4FBFC0E1108A8FD17B448A68554199C47D08FFB10D4B8")
            },
            Order = Convert.FromHexString("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEBAAEDCE6AF48A03BBFD25E8CD0364141"),
            Cofactor = Convert.FromHexString("01")
        };
    }
}
