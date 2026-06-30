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
    /// Creates and returns a Hash object that can be used to generate hash digests.
    /// </summary>
    /// <param name="algorithm">The algorithm to use (e.g., 'sha256', 'md5').</param>
    /// <returns>A Hash object.</returns>
    public static Hash createHash(string algorithm)
    {
        return new Hash(algorithm);
    }

    /// <summary>
    /// Creates and returns an Hmac object that uses the given algorithm and key.
    /// </summary>
    /// <param name="algorithm">The algorithm to use (e.g., 'sha256').</param>
    /// <param name="key">The HMAC key.</param>
    /// <returns>An Hmac object.</returns>
    public static Hmac createHmac(string algorithm, string key)
    {
        return new Hmac(algorithm, Encoding.UTF8.GetBytes(key));
    }

    /// <summary>
    /// Creates and returns an Hmac object that uses the given algorithm and key.
    /// </summary>
    /// <param name="algorithm">The algorithm to use (e.g., 'sha256').</param>
    /// <param name="key">The HMAC key as a byte array.</param>
    /// <returns>An Hmac object.</returns>
    public static Hmac createHmac(string algorithm, byte[] key)
    {
        return new Hmac(algorithm, key);
    }

    /// <summary>
    /// Creates and returns an Hmac object that uses the given algorithm and Buffer key.
    /// </summary>
    /// <param name="algorithm">The algorithm to use (e.g., 'sha256').</param>
    /// <param name="key">The HMAC key as a Buffer.</param>
    /// <returns>An Hmac object.</returns>
    public static Hmac createHmac(string algorithm, Buffer key)
    {
        if (key == null)
            throw new ArgumentNullException(nameof(key));

        return new Hmac(algorithm, key.InternalData);
    }

    /// <summary>
    /// Creates and returns a Cipher object with the given algorithm, key, and initialization vector.
    /// </summary>
    /// <param name="algorithm">The algorithm to use (e.g., 'aes-256-cbc').</param>
    /// <param name="key">The encryption key.</param>
    /// <param name="iv">The initialization vector.</param>
    /// <returns>A Cipher object.</returns>
    public static Cipher createCipheriv(string algorithm, byte[] key, byte[]? iv)
    {
        return new Cipher(algorithm, key, iv);
    }

    /// <summary>
    /// Creates and returns a Cipher object with the given algorithm, key, and initialization vector.
    /// </summary>
    /// <param name="algorithm">The algorithm to use (e.g., 'aes-256-cbc').</param>
    /// <param name="key">The encryption key as a string.</param>
    /// <param name="iv">The initialization vector as a string.</param>
    /// <returns>A Cipher object.</returns>
    public static Cipher createCipheriv(string algorithm, string key, string? iv)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var ivBytes = iv != null ? Encoding.UTF8.GetBytes(iv) : null;
        return new Cipher(algorithm, keyBytes, ivBytes);
    }

    /// <summary>
    /// Creates and returns a Decipher object with the given algorithm, key, and initialization vector.
    /// </summary>
    /// <param name="algorithm">The algorithm to use (e.g., 'aes-256-cbc').</param>
    /// <param name="key">The decryption key.</param>
    /// <param name="iv">The initialization vector.</param>
    /// <returns>A Decipher object.</returns>
    public static Decipher createDecipheriv(string algorithm, byte[] key, byte[]? iv)
    {
        return new Decipher(algorithm, key, iv);
    }

    /// <summary>
    /// Creates and returns a Decipher object with the given algorithm, key, and initialization vector.
    /// </summary>
    /// <param name="algorithm">The algorithm to use (e.g., 'aes-256-cbc').</param>
    /// <param name="key">The decryption key as a string.</param>
    /// <param name="iv">The initialization vector as a string.</param>
    /// <returns>A Decipher object.</returns>
    public static Decipher createDecipheriv(string algorithm, string key, string? iv)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var ivBytes = iv != null ? Encoding.UTF8.GetBytes(iv) : null;
        return new Decipher(algorithm, keyBytes, ivBytes);
    }

    /// <summary>
    /// Generates cryptographically strong pseudo-random data.
    /// </summary>
    /// <param name="size">The number of bytes to generate.</param>
    /// <returns>A byte array containing random bytes.</returns>
    public static byte[] randomBytes(int size)
    {
        var bytes = new byte[size];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return bytes;
    }

    /// <summary>
    /// Generates cryptographically strong pseudo-random data as a Buffer.
    /// </summary>
    /// <param name="size">The number of bytes to generate.</param>
    /// <returns>A Buffer containing random bytes.</returns>
    public static Buffer randomBytesBuffer(int size)
    {
        return Buffer.from(randomBytes(size));
    }

    /// <summary>
    /// Generates cryptographically strong pseudo-random data asynchronously.
    /// </summary>
    /// <param name="size">The number of bytes to generate.</param>
    /// <param name="callback">Callback function.</param>
    public static void randomBytes(int size, Action<Exception?, byte[]?> callback)
    {
        try
        {
            var bytes = randomBytes(size);
            callback(null, bytes);
        }
        catch (Exception ex)
        {
            callback(ex, null);
        }
    }

    /// <summary>
    /// Generates a random integer.
    /// </summary>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <returns>A random integer between 0 and max.</returns>
    public static int randomInt(int max)
    {
        return RandomNumberGenerator.GetInt32(max);
    }

    /// <summary>
    /// Generates a random integer.
    /// </summary>
    /// <param name="min">The minimum value (inclusive).</param>
    /// <param name="max">The maximum value (exclusive).</param>
    /// <returns>A random integer between min and max.</returns>
    public static int randomInt(int min, int max)
    {
        return RandomNumberGenerator.GetInt32(min, max);
    }

    /// <summary>
    /// Fills a buffer with random bytes.
    /// </summary>
    /// <param name="buffer">The buffer to fill.</param>
    /// <param name="offset">The offset to start filling.</param>
    /// <param name="size">The number of bytes to fill.</param>
    /// <returns>The filled buffer.</returns>
    public static byte[] randomFillSync(byte[] buffer, int offset = 0, int? size = null)
    {
        var actualSize = size ?? (buffer.Length - offset);
        var bytes = randomBytes(actualSize);
        Array.Copy(bytes, 0, buffer, offset, actualSize);
        return buffer;
    }

    /// <summary>
    /// Fills a Buffer with random bytes.
    /// </summary>
    /// <param name="buffer">The Buffer to fill.</param>
    /// <param name="offset">The offset to start filling.</param>
    /// <param name="size">The number of bytes to fill.</param>
    /// <returns>The filled Buffer.</returns>
    public static Buffer randomFillSync(Buffer buffer, int offset = 0, int? size = null)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));

        randomFillSync(buffer.InternalData, offset, size);
        return buffer;
    }

    /// <summary>
    /// Fills a buffer with random bytes asynchronously.
    /// </summary>
    /// <param name="buffer">The buffer to fill.</param>
    /// <param name="offset">The offset to start filling.</param>
    /// <param name="size">The number of bytes to fill.</param>
    /// <param name="callback">Callback function.</param>
    public static void randomFill(byte[] buffer, int offset, int size, Action<Exception?, byte[]?> callback)
    {
        try
        {
            randomFillSync(buffer, offset, size);
            callback(null, buffer);
        }
        catch (Exception ex)
        {
            callback(ex, null);
        }
    }

}
