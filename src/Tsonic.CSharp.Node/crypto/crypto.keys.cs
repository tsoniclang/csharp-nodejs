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
    /// Creates a KeyObject from a secret key.
    /// </summary>
    public static KeyObject createSecretKey(byte[] key)
    {
        return new SecretKeyObject(key);
    }

    /// <summary>
    /// Creates a KeyObject from a secret key.
    /// </summary>
    public static KeyObject createSecretKey(string key, string? encoding = null)
    {
        var enc = (encoding ?? "utf8").ToLowerInvariant();
        var keyBytes = enc switch
        {
            "hex" => Convert.FromHexString(key.Replace("-", "")),
            "base64" => Convert.FromBase64String(key),
            "base64url" => DecodeBase64Url(key),
            "utf8" or "utf-8" => Encoding.UTF8.GetBytes(key),
            "ascii" => Encoding.ASCII.GetBytes(key),
            "latin1" or "binary" => Encoding.Latin1.GetBytes(key),
            _ => Encoding.UTF8.GetBytes(key)
        };
        return new SecretKeyObject(keyBytes);
    }

    private static byte[] DecodeBase64Url(string base64url)
    {
        var base64 = base64url.Replace("-", "+").Replace("_", "/");
        // Add padding
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }

    /// <summary>
    /// Creates a public KeyObject from a key.
    /// </summary>
    public static KeyObject createPublicKey(string key)
    {
        try
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(key);
            return new PublicKeyObject(rsa, "rsa");
        }
        catch
        {
            try
            {
                var ecdsa = ECDsa.Create();
                ecdsa.ImportFromPem(key);
                return new PublicKeyObject(ecdsa, "ec");
            }
            catch
            {
                throw new ArgumentException("Unable to parse public key");
            }
        }
    }

    /// <summary>
    /// Creates a public KeyObject from a key.
    /// </summary>
    public static KeyObject createPublicKey(byte[] key)
    {
        try
        {
            var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(key, out _);
            return new PublicKeyObject(rsa, "rsa");
        }
        catch
        {
            try
            {
                var ecdsa = ECDsa.Create();
                ecdsa.ImportSubjectPublicKeyInfo(key, out _);
                return new PublicKeyObject(ecdsa, "ec");
            }
            catch
            {
                throw new ArgumentException("Unable to parse public key");
            }
        }
    }

    /// <summary>
    /// Creates a public KeyObject from another KeyObject.
    /// </summary>
    public static KeyObject createPublicKey(KeyObject key)
    {
        if (key.type == "public")
            return key;

        if (key.type != "private")
            throw new ArgumentException("Key must be a private or public key");

        // Extract public key from private key
        var exported = key.export();
        if (exported is string pemKey)
        {
            return createPublicKey(pemKey);
        }

        throw new ArgumentException("Unable to extract public key");
    }

    /// <summary>
    /// Creates a private KeyObject from a key.
    /// </summary>
    public static KeyObject createPrivateKey(string key)
    {
        try
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(key);
            return new PrivateKeyObject(rsa, "rsa");
        }
        catch
        {
            try
            {
                var ecdsa = ECDsa.Create();
                ecdsa.ImportFromPem(key);
                return new PrivateKeyObject(ecdsa, "ec");
            }
            catch
            {
                throw new ArgumentException("Unable to parse private key");
            }
        }
    }

    /// <summary>
    /// Creates a private KeyObject from a key.
    /// </summary>
    public static KeyObject createPrivateKey(byte[] key)
    {
        try
        {
            var rsa = RSA.Create();
            rsa.ImportPkcs8PrivateKey(key, out _);
            return new PrivateKeyObject(rsa, "rsa");
        }
        catch
        {
            try
            {
                var ecdsa = ECDsa.Create();
                ecdsa.ImportPkcs8PrivateKey(key, out _);
                return new PrivateKeyObject(ecdsa, "ec");
            }
            catch
            {
                throw new ArgumentException("Unable to parse private key");
            }
        }
    }

}
