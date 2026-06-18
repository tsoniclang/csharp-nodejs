using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace Tsonic.CSharp.Node;

/// <summary>
/// The Hmac class is a utility for creating cryptographic HMAC digests.
/// </summary>
public class Hmac : Transform
{
    private readonly HMAC _algorithm;
    private bool _finalized = false;

    internal Hmac(string algorithm, byte[] key)
    {
        _algorithm = CreateHmacAlgorithm(algorithm, key);
    }

    /// <summary>
    /// Updates the Hmac content with the given data.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="inputEncoding">The encoding of the data string.</param>
    /// <returns>The Hmac object for chaining.</returns>
    public Hmac update(string data, string? inputEncoding = null)
    {
        if (_finalized)
            throw new InvalidOperationException("Digest already called");

        var encoding = GetEncoding(inputEncoding ?? "utf8");
        var bytes = encoding.GetBytes(data);
        _algorithm.TransformBlock(bytes, 0, bytes.Length, null, 0);
        return this;
    }

    /// <summary>
    /// Updates the Hmac content with the given data.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The Hmac object for chaining.</returns>
    public Hmac update(byte[] data)
    {
        if (_finalized)
            throw new InvalidOperationException("Digest already called");

        _algorithm.TransformBlock(data, 0, data.Length, null, 0);
        return this;
    }

    /// <summary>
    /// Calculates the HMAC digest of all the data passed.
    /// </summary>
    /// <param name="encoding">The encoding of the return value.</param>
    /// <returns>The calculated HMAC.</returns>
    public string digest(string? encoding = null)
    {
        if (_finalized)
            throw new InvalidOperationException("Digest already called");

        _finalized = true;
        _algorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        var hash = _algorithm.Hash!;

        if (encoding == null || encoding == "buffer")
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        return encoding.ToLowerInvariant() switch
        {
            "hex" => BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant(),
            "base64" => Convert.ToBase64String(hash),
            "base64url" => Convert.ToBase64String(hash).Replace("+", "-").Replace("/", "_").TrimEnd('='),
            "latin1" or "binary" => Encoding.Latin1.GetString(hash),
            _ => throw new ArgumentException($"Unknown encoding: {encoding}")
        };
    }

    /// <summary>
    /// Calculates the HMAC digest of all the data passed and returns a Buffer.
    /// </summary>
    /// <returns>The calculated HMAC as a byte array.</returns>
    public byte[] digest()
    {
        if (_finalized)
            throw new InvalidOperationException("Digest already called");

        _finalized = true;
        _algorithm.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
        return _algorithm.Hash!;
    }

#pragma warning disable CS1591
    ~Hmac()
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
            _algorithm?.Dispose();
        }
    }
#pragma warning restore CS1591

    private static HMAC CreateHmacAlgorithm(string algorithm, byte[] key)
    {
        return algorithm.ToLowerInvariant() switch
        {
            "md5" => new HMACMD5(key),
            "sha1" or "sha-1" => new HMACSHA1(key),
            "sha256" or "sha-256" => new HMACSHA256(key),
            "sha384" or "sha-384" => new HMACSHA384(key),
            "sha512" or "sha-512" => new HMACSHA512(key),
            "sha512-224" => new BouncyCastleHMAC(new Sha512tDigest(224), key),
            "sha512-256" => new BouncyCastleHMAC(new Sha512tDigest(256), key),
            "sha3-224" => new BouncyCastleHMAC(new Sha3Digest(224), key),
            "sha3-256" => new BouncyCastleHMAC(new Sha3Digest(256), key),
            "sha3-384" => new BouncyCastleHMAC(new Sha3Digest(384), key),
            "sha3-512" => new BouncyCastleHMAC(new Sha3Digest(512), key),
            "ripemd160" or "rmd160" => new BouncyCastleHMAC(new RipeMD160Digest(), key),
            "blake2b512" => new BouncyCastleHMAC(new Blake2bDigest(512), key),
            "blake2s256" => new BouncyCastleHMAC(new Blake2sDigest(256), key),
            _ => throw new ArgumentException($"Unknown HMAC algorithm: {algorithm}")
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

/// <summary>
/// Wrapper to adapt BouncyCastle HMac to .NET HMAC.
/// </summary>
internal class BouncyCastleHMAC : HMAC
{
    private readonly HMac _hmac;
    private byte[]? _hashValue;

    public BouncyCastleHMAC(Org.BouncyCastle.Crypto.IDigest digest, byte[] key)
    {
        _hmac = new HMac(digest);
        _hmac.Init(new KeyParameter(key));
        HashSizeValue = digest.GetDigestSize() * 8;
        Key = key;
    }

    public override void Initialize()
    {
        _hmac.Reset();
        _hashValue = null;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
        _hmac.BlockUpdate(array, ibStart, cbSize);
    }

    protected override byte[] HashFinal()
    {
        _hashValue = new byte[_hmac.GetMacSize()];
        _hmac.DoFinal(_hashValue, 0);
        return _hashValue;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hmac.Reset();
        }
        base.Dispose(disposing);
    }
}
