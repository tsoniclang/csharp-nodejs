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
    private readonly IncrementalHash? _native;
    private readonly HMac? _digest;
    private bool _finalized = false;

    internal Hmac(string algorithm, ReadOnlySpan<byte> key)
    {
        var name = algorithm.ToLowerInvariant();
        var nativeName = name switch
        {
            "md5" => HashAlgorithmName.MD5,
            "sha1" or "sha-1" => HashAlgorithmName.SHA1,
            "sha256" or "sha-256" => HashAlgorithmName.SHA256,
            "sha384" or "sha-384" => HashAlgorithmName.SHA384,
            "sha512" or "sha-512" => HashAlgorithmName.SHA512,
            _ => default
        };
        if (nativeName.Name is not null)
            _native = IncrementalHash.CreateHMAC(nativeName, key);
        else
        {
            Org.BouncyCastle.Crypto.IDigest digest = name switch
            {
                "sha512-224" => new Sha512tDigest(224),
                "sha512-256" => new Sha512tDigest(256),
                "sha3-224" => new Sha3Digest(224),
                "sha3-256" => new Sha3Digest(256),
                "sha3-384" => new Sha3Digest(384),
                "sha3-512" => new Sha3Digest(512),
                "ripemd160" or "rmd160" => new RipeMD160Digest(),
                "blake2b512" => new Blake2bDigest(512),
                "blake2s256" => new Blake2sDigest(256),
                _ => throw new ArgumentException($"Unknown HMAC algorithm: {algorithm}")
            };
            _digest = new HMac(digest);
            _digest.Init(new KeyParameter(key));
        }
    }

    private Hmac Update(ReadOnlySpan<byte> data)
    {
        if (_finalized)
            throw new InvalidOperationException("Digest already called");
        if (_native is not null)
            _native.AppendData(data);
        else
            _digest!.BlockUpdate(data);
        return this;
    }

    private byte[] Finish()
    {
        if (_finalized)
            throw new InvalidOperationException("Digest already called");
        _finalized = true;
        if (_native is not null)
            return _native.GetHashAndReset();
        var output = new byte[_digest!.GetMacSize()];
        _digest.DoFinal(output, 0);
        return output;
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
        return Update(bytes);
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

        return Update(data);
    }

    /// <summary>
    /// Updates the Hmac content with the given Buffer.
    /// </summary>
    /// <param name="data">The Buffer to hash.</param>
    /// <returns>The Hmac object for chaining.</returns>
    public Hmac update(Buffer data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        return Update(data.InternalMemory.Span);
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

        var hash = Finish();

        if (encoding == null || encoding == "buffer")
        {
            return Convert.ToHexStringLower(hash);
        }

        return encoding.ToLowerInvariant() switch
        {
            "hex" => Convert.ToHexStringLower(hash),
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

        return Finish();
    }

    /// <summary>
    /// Calculates the HMAC digest of all the data passed and returns a Buffer.
    /// </summary>
    /// <returns>The calculated HMAC as a Buffer.</returns>
    public Buffer digestBuffer()
    {
        return Buffer.TakeOwnership(digest());
    }

#pragma warning disable CS1591
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void Dispose(bool disposing)
    {
        if (disposing)
        {
            _native?.Dispose();
            _digest?.Reset();
            _finalized = true;
        }
    }
#pragma warning restore CS1591

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
