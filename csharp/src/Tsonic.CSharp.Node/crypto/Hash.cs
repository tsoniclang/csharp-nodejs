using System;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Crypto.Digests;

namespace Tsonic.CSharp.Node;

/// <summary>
/// The Hash class is a utility for creating hash digests of data.
/// </summary>
public class Hash : Transform
{
    private readonly IncrementalHash? _native;
    private readonly IDigest? _digest;
    private bool _finalized;

    internal Hash(string algorithm)
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
            _native = IncrementalHash.CreateHash(nativeName);
        else
            _digest = name switch
            {
                "sha512-224" => new Sha512tDigest(224),
                "sha512-256" => new Sha512tDigest(256),
                "sha3-224" => new Sha3Digest(224),
                "sha3-256" => new Sha3Digest(256),
                "sha3-384" => new Sha3Digest(384),
                "sha3-512" => new Sha3Digest(512),
                "shake128" => new ShakeDigest(128),
                "shake256" => new ShakeDigest(256),
                "ripemd160" or "rmd160" => new RipeMD160Digest(),
                "blake2b512" => new Blake2bDigest(512),
                "blake2s256" => new Blake2sDigest(256),
                _ => throw new ArgumentException($"Unknown hash algorithm: {algorithm}")
            };
    }

    private Hash(IncrementalHash? native, IDigest? digest)
    {
        _native = native;
        _digest = digest;
    }

    internal Hash Update(ReadOnlySpan<byte> data)
    {
        if (_finalized)
            throw new InvalidOperationException("Digest already called");
        if (_native is not null)
            _native.AppendData(data);
        else
            _digest!.BlockUpdate(data);
        return this;
    }

    /// <summary>
    /// Updates the hash content with the given data.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <param name="inputEncoding">The encoding of the data string.</param>
    /// <returns>The Hash object for chaining.</returns>
    public Hash update(string data, string? inputEncoding = null)
    {
        if (_finalized)
            throw new InvalidOperationException("Digest already called");

        var encoding = GetEncoding(inputEncoding ?? "utf8");
        var bytes = encoding.GetBytes(data);
        return Update(bytes);
    }

    /// <summary>
    /// Updates the hash content with the given data.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The Hash object for chaining.</returns>
    public Hash update(byte[] data)
    {
        if (_finalized)
            throw new InvalidOperationException("Digest already called");

        return Update(data);
    }

    /// <summary>
    /// Updates the hash content with the given Buffer.
    /// </summary>
    /// <param name="data">The Buffer to hash.</param>
    /// <returns>The Hash object for chaining.</returns>
    public Hash update(Buffer data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));

        return Update(data.InternalMemory.Span);
    }

    /// <summary>
    /// Calculates the digest of all the data passed to be hashed.
    /// </summary>
    /// <param name="encoding">The encoding of the return value.</param>
    /// <returns>The calculated hash.</returns>
    public string digest(string? encoding)
    {
        var hash = digestBytes();

        if (encoding == null || encoding == "buffer")
        {
            // Return hex by default
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
    /// Calculates the digest of all the data passed to be hashed and returns a Buffer.
    /// </summary>
    /// <returns>The calculated hash as a byte array.</returns>
    public byte[] digest()
    {
        return digestBytes();
    }

    /// <summary>
    /// Calculates the digest of all the data passed to be hashed and returns a Buffer.
    /// </summary>
    /// <returns>The calculated hash as a Buffer.</returns>
    public Buffer digestBuffer()
    {
        return Buffer.TakeOwnership(digestBytes());
    }

    /// <summary>
    /// Calculates the digest with SHAKE output length control.
    /// </summary>
    /// <param name="outputLength">For SHAKE algorithms, the output length in bytes.</param>
    /// <returns>The calculated hash as a byte array.</returns>
    public byte[] digest(int outputLength)
    {
        return digestBytes(outputLength);
    }

    private byte[] digestBytes(int? outputLength = null)
    {
        if (_finalized)
            throw new InvalidOperationException("Digest already called");

        _finalized = true;

        if (_native is not null)
            return _native.GetHashAndReset();
        if (_digest is ShakeDigest shake)
        {
            var length = outputLength ?? (shake.AlgorithmName == "SHAKE128" ? 16 : 32);
            var output = new byte[length];
            shake.OutputFinal(output, 0, length);
            return output;
        }
        var hash = new byte[_digest!.GetDigestSize()];
        _digest.DoFinal(hash, 0);
        return hash;
    }

    /// <summary>
    /// Creates a copy of the Hash object in its current state.
    /// </summary>
    /// <returns>A new Hash object.</returns>
    public Hash copy()
    {
        if (_finalized)
            throw new InvalidOperationException("Cannot copy finalized hash");

        if (_native is not null)
            return new Hash(_native.Clone(), null);
        var copied = _digest switch
        {
            Blake2bDigest digest => new Blake2bDigest(digest),
            Blake2sDigest digest => new Blake2sDigest(digest),
            IMemoable digest => (IDigest)digest.Copy(),
            _ => throw new InvalidOperationException("Hash state does not support copying")
        };
        return new Hash(null, copied);
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
            "base64" => Encoding.ASCII, // Base64 is ASCII-based
            "hex" => Encoding.ASCII, // Hex is ASCII-based
            _ => Encoding.UTF8
        };
    }
}
