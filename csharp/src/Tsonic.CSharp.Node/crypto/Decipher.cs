using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Instances of the Decipher class are used to decrypt data.
/// </summary>
public class Decipher : Transform
{
    private readonly ICryptoTransform? _decryptor;
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

    internal Decipher(string algorithm, byte[] key, byte[]? iv)
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
            var (cipher, transform) = CreateDecipher(algorithm, key, iv);
            _decryptor = transform;
            _memoryStream = new MemoryStream();
            _cryptoStream = new CryptoStream(_memoryStream, _decryptor, CryptoStreamMode.Write);
        }
    }

    /// <summary>
    /// Updates the decipher with data.
    /// </summary>
    /// <param name="data">The data to decrypt.</param>
    /// <param name="inputEncoding">The encoding of the data.</param>
    /// <param name="outputEncoding">The encoding of the return value.</param>
    /// <returns>The decrypted data.</returns>
    public string update(string data, string? inputEncoding = null, string? outputEncoding = null)
    {
        if (_finalized)
            throw new InvalidOperationException("Decipher already finalized");

        byte[] bytes;
        var inEnc = (inputEncoding ?? "base64").ToLowerInvariant();

        bytes = inEnc switch
        {
            "hex" => Convert.FromHexString(data.Replace("-", "")),
            "base64" => Convert.FromBase64String(data),
            "base64url" => Convert.FromBase64String(data.Replace("-", "+").Replace("_", "/")),
            "latin1" or "binary" => Encoding.Latin1.GetBytes(data),
            _ => Encoding.UTF8.GetBytes(data)
        };

        return update(bytes, outputEncoding);
    }

    /// <summary>
    /// Updates the decipher with data.
    /// </summary>
    /// <param name="data">The data to decrypt.</param>
    /// <param name="outputEncoding">The encoding of the return value.</param>
    /// <returns>The decrypted data.</returns>
    public string update(byte[] data, string? outputEncoding = null)
    {
        if (_finalized)
            throw new InvalidOperationException("Decipher already finalized");

        if (_isGcmMode)
        {
            // GCM mode: buffer data until final()
            _gcmDataBuffer!.Write(data, 0, data.Length);
            return ""; // GCM returns empty string on update
        }

        _cryptoStream!.Write(data, 0, data.Length);
        var decrypted = _memoryStream!.ToArray();
        _memoryStream.SetLength(0);

        var outEnc = (outputEncoding ?? "utf8").ToLowerInvariant();
        return outEnc switch
        {
            "hex" => BitConverter.ToString(decrypted).Replace("-", "").ToLowerInvariant(),
            "base64" => Convert.ToBase64String(decrypted),
            "base64url" => Convert.ToBase64String(decrypted).Replace("+", "-").Replace("/", "_").TrimEnd('='),
            "latin1" or "binary" => Encoding.Latin1.GetString(decrypted),
            "utf8" or "utf-8" => Encoding.UTF8.GetString(decrypted),
            "ascii" => Encoding.ASCII.GetString(decrypted),
            _ => Encoding.UTF8.GetString(decrypted)
        };
    }

    /// <summary>
    /// Returns any remaining deciphered contents.
    /// </summary>
    /// <param name="outputEncoding">The encoding of the return value.</param>
    /// <returns>The remaining decrypted data.</returns>
    public string final(string? outputEncoding = null)
    {
        var decrypted = final();

        var outEnc = (outputEncoding ?? "utf8").ToLowerInvariant();
        return outEnc switch
        {
            "hex" => BitConverter.ToString(decrypted).Replace("-", "").ToLowerInvariant(),
            "base64" => Convert.ToBase64String(decrypted),
            "base64url" => Convert.ToBase64String(decrypted).Replace("+", "-").Replace("/", "_").TrimEnd('='),
            "latin1" or "binary" => Encoding.Latin1.GetString(decrypted),
            "utf8" or "utf-8" => Encoding.UTF8.GetString(decrypted),
            "ascii" => Encoding.ASCII.GetString(decrypted),
            _ => Encoding.UTF8.GetString(decrypted)
        };
    }

    /// <summary>
    /// Returns any remaining deciphered contents as a Buffer.
    /// </summary>
    /// <returns>The remaining decrypted data.</returns>
    public byte[] final()
    {
        if (_finalized)
            throw new InvalidOperationException("Decipher already finalized");

        _finalized = true;

        if (_isGcmMode)
        {
            // GCM decryption
            if (_gcmTag == null)
                throw new InvalidOperationException("Must call setAuthTag() before final() for GCM mode");

            var ciphertext = _gcmDataBuffer!.ToArray();
            var plaintext = new byte[ciphertext.Length];

            using var aesGcm = new AesGcm(_gcmKey!, AesGcm.TagByteSizes.MaxSize);
            aesGcm.Decrypt(_gcmNonce!, ciphertext, _gcmTag, plaintext, _gcmAad);

            return plaintext;
        }

        _cryptoStream!.FlushFinalBlock();
        return _memoryStream!.ToArray();
    }

    /// <summary>
    /// When using an authenticated encryption mode, sets the authentication tag.
    /// </summary>
    /// <param name="buffer">The authentication tag.</param>
    public void setAuthTag(byte[] buffer)
    {
        if (!_isGcmMode)
            throw new NotSupportedException("setAuthTag is only supported for authenticated cipher modes.");

        if (_finalized)
            throw new InvalidOperationException("Cannot set auth tag after finalization");

        _gcmTag = buffer;
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
    ~Decipher()
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
            _decryptor?.Dispose();
            _gcmDataBuffer?.Dispose();
        }
    }
#pragma warning restore CS1591

    private static (SymmetricAlgorithm, ICryptoTransform) CreateDecipher(string algorithm, byte[] key, byte[]? iv)
    {
        var alg = algorithm.ToLowerInvariant();

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
            return (aes, aes.CreateDecryptor());
        }
        else if (alg.StartsWith("des-ede3") || alg == "des3")
        {
            var des3 = TripleDES.Create();
            des3.Key = key;
            if (iv != null) des3.IV = iv;
            des3.Mode = alg.Contains("-ecb") ? CipherMode.ECB : CipherMode.CBC;
            des3.Padding = PaddingMode.PKCS7;
            return (des3, des3.CreateDecryptor());
        }
        else if (alg.StartsWith("des-"))
        {
            var des = DES.Create();
            des.Key = key;
            if (iv != null) des.IV = iv;
            des.Mode = alg.Contains("-ecb") ? CipherMode.ECB : CipherMode.CBC;
            des.Padding = PaddingMode.PKCS7;
            return (des, des.CreateDecryptor());
        }
        else if (alg.StartsWith("rc2-"))
        {
            var rc2 = RC2.Create();
            rc2.Key = key;
            if (iv != null) rc2.IV = iv;
            rc2.Mode = alg.Contains("-ecb") ? CipherMode.ECB : CipherMode.CBC;
            rc2.Padding = PaddingMode.PKCS7;
            return (rc2, rc2.CreateDecryptor());
        }

        throw new ArgumentException($"Unknown or unsupported cipher algorithm: {algorithm}");
    }
}
