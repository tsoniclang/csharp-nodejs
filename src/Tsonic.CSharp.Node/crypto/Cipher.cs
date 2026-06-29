using System;
using System.Security.Cryptography;
using System.Text;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Instances of the Cipher class are used to encrypt data.
/// </summary>
public class Cipher : Transform
{
    private readonly ICryptoTransform? _encryptor;
    private readonly MemoryStream? _memoryStream;
    private readonly CryptoStream? _cryptoStream;
    private bool _finalized = false;

    // GCM mode fields
    private readonly bool _isGcmMode = false;
    private readonly byte[]? _gcmKey;
    private readonly byte[]? _gcmNonce;
    private byte[]? _gcmAad;
    private byte[]? _gcmTag;
    private readonly MemoryStream? _gcmDataBuffer;

    internal Cipher(string algorithm, byte[] key, byte[]? iv)
    {
        var alg = algorithm.ToLowerInvariant();

        // Check if GCM mode
        if (alg.Contains("-gcm"))
        {
            _isGcmMode = true;
            _gcmKey = key;
            _gcmNonce = iv ?? throw new ArgumentNullException(nameof(iv), "GCM mode requires nonce/IV");
            _gcmDataBuffer = new MemoryStream();
        }
        else
        {
            var (cipher, transform) = CreateCipher(algorithm, key, iv);
            _encryptor = transform;
            _memoryStream = new MemoryStream();
            _cryptoStream = new CryptoStream(_memoryStream, _encryptor, CryptoStreamMode.Write);
        }
    }

    /// <summary>
    /// Updates the cipher with data.
    /// </summary>
    /// <param name="data">The data to encrypt.</param>
    /// <param name="inputEncoding">The encoding of the data.</param>
    /// <param name="outputEncoding">The encoding of the return value.</param>
    /// <returns>The encrypted data.</returns>
    public string update(string data, string? inputEncoding = null, string? outputEncoding = null)
    {
        if (_finalized)
            throw new InvalidOperationException("Cipher already finalized");

        var encoding = GetEncoding(inputEncoding ?? "utf8");
        var bytes = encoding.GetBytes(data);
        return update(bytes, outputEncoding);
    }

    /// <summary>
    /// Updates the cipher with data.
    /// </summary>
    /// <param name="data">The data to encrypt.</param>
    /// <param name="outputEncoding">The encoding of the return value.</param>
    /// <returns>The encrypted data.</returns>
    public string update(byte[] data, string? outputEncoding = null)
    {
        if (_finalized)
            throw new InvalidOperationException("Cipher already finalized");

        if (_isGcmMode)
        {
            // GCM mode: buffer data until final()
            _gcmDataBuffer!.Write(data, 0, data.Length);
            return ""; // GCM returns empty string on update
        }

        _cryptoStream!.Write(data, 0, data.Length);
        var encrypted = _memoryStream!.ToArray();
        _memoryStream.SetLength(0);

        if (outputEncoding == null || outputEncoding == "buffer")
        {
            return Convert.ToBase64String(encrypted);
        }

        return outputEncoding.ToLowerInvariant() switch
        {
            "hex" => BitConverter.ToString(encrypted).Replace("-", "").ToLowerInvariant(),
            "base64" => Convert.ToBase64String(encrypted),
            "base64url" => Convert.ToBase64String(encrypted).Replace("+", "-").Replace("/", "_").TrimEnd('='),
            "latin1" or "binary" => Encoding.Latin1.GetString(encrypted),
            _ => throw new ArgumentException($"Unknown encoding: {outputEncoding}")
        };
    }

    /// <summary>
    /// Returns any remaining enciphered contents.
    /// </summary>
    /// <param name="outputEncoding">The encoding of the return value.</param>
    /// <returns>The remaining encrypted data.</returns>
    public string final(string? outputEncoding = null)
    {
        var encrypted = final();

        if (outputEncoding == null || outputEncoding == "buffer")
        {
            return Convert.ToBase64String(encrypted);
        }

        return outputEncoding.ToLowerInvariant() switch
        {
            "hex" => BitConverter.ToString(encrypted).Replace("-", "").ToLowerInvariant(),
            "base64" => Convert.ToBase64String(encrypted),
            "base64url" => Convert.ToBase64String(encrypted).Replace("+", "-").Replace("/", "_").TrimEnd('='),
            "latin1" or "binary" => Encoding.Latin1.GetString(encrypted),
            _ => throw new ArgumentException($"Unknown encoding: {outputEncoding}")
        };
    }

    /// <summary>
    /// Returns any remaining enciphered contents as a Buffer.
    /// </summary>
    /// <returns>The remaining encrypted data.</returns>
    public byte[] final()
    {
        if (_finalized)
            throw new InvalidOperationException("Cipher already finalized");

        _finalized = true;

        if (_isGcmMode)
        {
            // GCM encryption
            var plaintext = _gcmDataBuffer!.ToArray();
            var ciphertext = new byte[plaintext.Length];
            _gcmTag = new byte[16]; // AES-GCM uses 128-bit (16 byte) tag

            using var aesGcm = new AesGcm(_gcmKey!, AesGcm.TagByteSizes.MaxSize);
            aesGcm.Encrypt(_gcmNonce!, plaintext, ciphertext, _gcmTag, _gcmAad);

            return ciphertext;
        }

        _cryptoStream!.FlushFinalBlock();
        return _memoryStream!.ToArray();
    }

    /// <summary>
    /// When using an authenticated encryption mode, sets the length of the authentication tag.
    /// </summary>
    /// <param name="tagLength">The tag length in bytes.</param>
    public void setAuthTag(int tagLength)
    {
        if (!_isGcmMode)
            throw new NotSupportedException("setAuthTag is only supported for authenticated cipher modes.");

        // Note: AesGcm always uses 16-byte tag, but this method is here for API compatibility
    }

    /// <summary>
    /// When using an authenticated encryption mode, returns the authentication tag.
    /// </summary>
    /// <returns>The authentication tag.</returns>
    public byte[] getAuthTag()
    {
        if (!_isGcmMode)
            throw new NotSupportedException("getAuthTag is only supported for authenticated cipher modes.");

        if (!_finalized)
            throw new InvalidOperationException("Must call final() before getAuthTag()");

        if (_gcmTag == null)
            throw new InvalidOperationException("No auth tag available");

        return _gcmTag;
    }

    /// <summary>
    /// When using an authenticated encryption mode, sets AAD (Additional Authenticated Data).
    /// </summary>
    /// <param name="buffer">The AAD data.</param>
    public void setAAD(byte[] buffer)
    {
        if (!_isGcmMode)
            throw new NotSupportedException("setAAD is only supported for authenticated cipher modes.");

        if (_finalized)
            throw new InvalidOperationException("Cannot set AAD after finalization");

        _gcmAad = buffer;
    }

#pragma warning disable CS1591
    ~Cipher()
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
            _cryptoStream?.Dispose();
            _memoryStream?.Dispose();
            _encryptor?.Dispose();
            _gcmDataBuffer?.Dispose();
        }
    }
#pragma warning restore CS1591

    private static (SymmetricAlgorithm, ICryptoTransform) CreateCipher(string algorithm, byte[] key, byte[]? iv)
    {
        var alg = algorithm.ToLowerInvariant();

        // Parse algorithm name (e.g., "aes-256-cbc", "des-ede3-cbc")
        if (alg.StartsWith("aes-"))
        {
            var aes = Aes.Create();
            aes.Key = key;
            if (iv != null) aes.IV = iv;

            if (alg.Contains("-ecb")) aes.Mode = CipherMode.ECB;
            else if (alg.Contains("-cbc")) aes.Mode = CipherMode.CBC;
            else if (alg.Contains("-cfb")) aes.Mode = CipherMode.CFB;
            else if (alg.Contains("-ctr")) aes.Mode = CipherMode.ECB; // CTR not directly supported
            else if (alg.Contains("-gcm"))
            {
                // GCM mode is handled separately in constructor, should not reach here
                throw new InvalidOperationException("GCM mode should be handled in constructor");
            }
            else aes.Mode = CipherMode.CBC; // default

            aes.Padding = PaddingMode.PKCS7;
            return (aes, aes.CreateEncryptor());
        }
        else if (alg.StartsWith("des-ede3") || alg == "des3")
        {
            var des3 = TripleDES.Create();
            des3.Key = key;
            if (iv != null) des3.IV = iv;
            des3.Mode = alg.Contains("-ecb") ? CipherMode.ECB : CipherMode.CBC;
            des3.Padding = PaddingMode.PKCS7;
            return (des3, des3.CreateEncryptor());
        }
        else if (alg.StartsWith("des-"))
        {
            var des = DES.Create();
            des.Key = key;
            if (iv != null) des.IV = iv;
            des.Mode = alg.Contains("-ecb") ? CipherMode.ECB : CipherMode.CBC;
            des.Padding = PaddingMode.PKCS7;
            return (des, des.CreateEncryptor());
        }
        else if (alg.StartsWith("rc2-"))
        {
            var rc2 = RC2.Create();
            rc2.Key = key;
            if (iv != null) rc2.IV = iv;
            rc2.Mode = alg.Contains("-ecb") ? CipherMode.ECB : CipherMode.CBC;
            rc2.Padding = PaddingMode.PKCS7;
            return (rc2, rc2.CreateEncryptor());
        }

        throw new ArgumentException($"Unknown or unsupported cipher algorithm: {algorithm}");
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
