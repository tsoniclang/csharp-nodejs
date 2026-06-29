using System;
using System.Security.Cryptography;
using System.Text;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Represents a cryptographic key.
/// </summary>
public abstract class KeyObject : IDisposable
{
    /// <summary>
    /// The type of the key: 'secret', 'public', or 'private'.
    /// </summary>
    public abstract string type { get; }

    /// <summary>
    /// For asymmetric keys, this property returns the type of the key (e.g., 'rsa', 'ec', 'ed25519').
    /// For secret keys, this property is undefined.
    /// </summary>
    public abstract string? asymmetricKeyType { get; }

    /// <summary>
    /// For secret keys, this property returns the size of the key in bytes.
    /// For asymmetric keys, this property is undefined.
    /// </summary>
    public abstract int? symmetricKeySize { get; }

    /// <summary>
    /// Whether the key can be exported.
    /// </summary>
    public virtual bool extractable => true;

    /// <summary>
    /// WebCrypto-compatible key usages.
    /// </summary>
    public virtual string[] keyUsages => [];

    /// <summary>
    /// WebCrypto-compatible algorithm metadata.
    /// </summary>
    public virtual KeyAlgorithm? algorithm => asymmetricKeyType == null ? null : new KeyAlgorithm { name = asymmetricKeyType };

    /// <summary>
    /// Asymmetric key details when available.
    /// </summary>
    public virtual AsymmetricKeyDetails? asymmetricKeyDetails => null;

    /// <summary>
    /// Compares exported key material for equality.
    /// </summary>
    public virtual bool equals(KeyObject other)
    {
        if (other == null)
            return false;

        var left = export();
        var right = other.export();
        if (left is byte[] leftBytes && right is byte[] rightBytes)
            return crypto.timingSafeEqual(leftBytes, rightBytes);

        return string.Equals(left?.ToString(), right?.ToString(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Exports the key in the specified format.
    /// </summary>
    public abstract object export(object? options = null);

    /// <summary>
    /// Disposes of the key object.
    /// </summary>
    public abstract void Dispose();
}

/// <summary>
/// Represents a secret (symmetric) key.
/// </summary>
public class SecretKeyObject : KeyObject
{
    private byte[] _keyData;
    private bool _disposed = false;

    internal SecretKeyObject(byte[] keyData)
    {
        _keyData = keyData ?? throw new ArgumentNullException(nameof(keyData));
    }

#pragma warning disable CS1591
    public override string type => "secret";
    public override string? asymmetricKeyType => null;
    public override int? symmetricKeySize => _keyData.Length;
#pragma warning restore CS1591

    /// <summary>
    /// Exports the secret key.
    /// </summary>
    public override object export(object? options = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SecretKeyObject));

        return _keyData;
    }

    /// <summary>
    /// Exports the secret key as a byte array.
    /// </summary>
    public byte[] export()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(SecretKeyObject));

        return _keyData;
    }

#pragma warning disable CS1591
    public override void Dispose()
    {
        if (!_disposed)
        {
            Array.Clear(_keyData, 0, _keyData.Length);
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
#pragma warning restore CS1591
}

/// <summary>
/// Represents a public key.
/// </summary>
public class PublicKeyObject : KeyObject
{
    private readonly AsymmetricAlgorithm _key;
    private readonly string _keyType;
    private bool _disposed = false;

    internal PublicKeyObject(AsymmetricAlgorithm key, string keyType)
    {
        _key = key ?? throw new ArgumentNullException(nameof(key));
        _keyType = keyType ?? throw new ArgumentNullException(nameof(keyType));
    }

    /// <summary>
    /// Gets the underlying asymmetric algorithm for internal use.
    /// </summary>
    internal AsymmetricAlgorithm GetKey()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PublicKeyObject));
        return _key;
    }

#pragma warning disable CS1591
    public override string type => "public";
    public override string? asymmetricKeyType => _keyType;
    public override int? symmetricKeySize => null;
    public override AsymmetricKeyDetails? asymmetricKeyDetails => _key is RSA rsa ? new AsymmetricKeyDetails { modulusLength = rsa.KeySize } : null;
#pragma warning restore CS1591

    /// <summary>
    /// Exports the public key in PEM or DER format.
    /// </summary>
    public override object export(object? options = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PublicKeyObject));

        // Default to PEM SPKI format
        if (_key is RSA rsa)
        {
            return rsa.ExportSubjectPublicKeyInfoPem();
        }
        else if (_key is ECDsa ecdsa)
        {
            return ecdsa.ExportSubjectPublicKeyInfoPem();
        }
        else if (_key is ECDiffieHellman ecdh)
        {
            return Encoding.UTF8.GetString(ecdh.PublicKey.ExportSubjectPublicKeyInfo());
        }

        throw new NotSupportedException($"Export not supported for key type {_keyType}");
    }

    /// <summary>
    /// Exports the public key in the specified format.
    /// </summary>
    public string export(string format, string? type = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PublicKeyObject));

        var fmt = format.ToLowerInvariant();
        var keyType = (type ?? "spki").ToLowerInvariant();

        if (_key is RSA rsa)
        {
            return fmt switch
            {
                "pem" => keyType == "spki" ? rsa.ExportSubjectPublicKeyInfoPem() : rsa.ExportRSAPublicKeyPem(),
                "der" => Convert.ToBase64String(keyType == "spki" ? rsa.ExportSubjectPublicKeyInfo() : rsa.ExportRSAPublicKey()),
                _ => throw new ArgumentException($"Unknown format: {format}")
            };
        }
        else if (_key is ECDsa ecdsa)
        {
            return fmt switch
            {
                "pem" => ecdsa.ExportSubjectPublicKeyInfoPem(),
                "der" => Convert.ToBase64String(ecdsa.ExportSubjectPublicKeyInfo()),
                _ => throw new ArgumentException($"Unknown format: {format}")
            };
        }

        throw new NotSupportedException($"Export not supported for key type {_keyType}");
    }

#pragma warning disable CS1591
    public override void Dispose()
    {
        if (!_disposed)
        {
            _key?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
#pragma warning restore CS1591
}

/// <summary>
/// Represents a private key.
/// </summary>
public class PrivateKeyObject : KeyObject
{
    private readonly AsymmetricAlgorithm _key;
    private readonly string _keyType;
    private bool _disposed = false;

    internal PrivateKeyObject(AsymmetricAlgorithm key, string keyType)
    {
        _key = key ?? throw new ArgumentNullException(nameof(key));
        _keyType = keyType ?? throw new ArgumentNullException(nameof(keyType));
    }

    /// <summary>
    /// Gets the underlying asymmetric algorithm for internal use.
    /// </summary>
    internal AsymmetricAlgorithm GetKey()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PrivateKeyObject));
        return _key;
    }

#pragma warning disable CS1591
    public override string type => "private";
    public override string? asymmetricKeyType => _keyType;
    public override int? symmetricKeySize => null;
    public override AsymmetricKeyDetails? asymmetricKeyDetails => _key is RSA rsa ? new AsymmetricKeyDetails { modulusLength = rsa.KeySize } : null;
#pragma warning restore CS1591

    /// <summary>
    /// Exports the private key in PEM or DER format.
    /// </summary>
    public override object export(object? options = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PrivateKeyObject));

        // Default to PEM PKCS8 format
        if (_key is RSA rsa)
        {
            return rsa.ExportPkcs8PrivateKeyPem();
        }
        else if (_key is ECDsa ecdsa)
        {
            return ecdsa.ExportPkcs8PrivateKeyPem();
        }
        else if (_key is ECDiffieHellman ecdh)
        {
            return Encoding.UTF8.GetString(ecdh.ExportPkcs8PrivateKey());
        }

        throw new NotSupportedException($"Export not supported for key type {_keyType}");
    }

    /// <summary>
    /// Exports the private key in the specified format.
    /// </summary>
    public string export(string format, string? type = null, string? cipher = null, string? passphrase = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PrivateKeyObject));

        var fmt = format.ToLowerInvariant();
        var keyType = (type ?? "pkcs8").ToLowerInvariant();

        if (_key is RSA rsa)
        {
            if (cipher != null && passphrase != null)
            {
                // Encrypted export
                var pbe = new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 2048);
                var privateKeyBytes = keyType == "pkcs8" ? rsa.ExportEncryptedPkcs8PrivateKey(passphrase, pbe) : rsa.ExportRSAPrivateKey();
                return fmt switch
                {
                    "pem" => Encoding.UTF8.GetString(privateKeyBytes),
                    "der" => Convert.ToBase64String(privateKeyBytes),
                    _ => throw new ArgumentException($"Unknown format: {format}")
                };
            }

            return fmt switch
            {
                "pem" => keyType == "pkcs8" ? rsa.ExportPkcs8PrivateKeyPem() : rsa.ExportRSAPrivateKeyPem(),
                "der" => Convert.ToBase64String(keyType == "pkcs8" ? rsa.ExportPkcs8PrivateKey() : rsa.ExportRSAPrivateKey()),
                _ => throw new ArgumentException($"Unknown format: {format}")
            };
        }
        else if (_key is ECDsa ecdsa)
        {
            if (cipher != null && passphrase != null)
            {
                // Encrypted export
                var pbe = new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 2048);
                var privateKeyBytes = ecdsa.ExportEncryptedPkcs8PrivateKey(passphrase, pbe);
                return fmt switch
                {
                    "pem" => Encoding.UTF8.GetString(privateKeyBytes),
                    "der" => Convert.ToBase64String(privateKeyBytes),
                    _ => throw new ArgumentException($"Unknown format: {format}")
                };
            }

            return fmt switch
            {
                "pem" => keyType == "pkcs8" ? ecdsa.ExportPkcs8PrivateKeyPem() : ecdsa.ExportECPrivateKeyPem(),
                "der" => Convert.ToBase64String(keyType == "pkcs8" ? ecdsa.ExportPkcs8PrivateKey() : ecdsa.ExportECPrivateKey()),
                _ => throw new ArgumentException($"Unknown format: {format}")
            };
        }

        throw new NotSupportedException($"Export not supported for key type {_keyType}");
    }

#pragma warning disable CS1591
    public override void Dispose()
    {
        if (!_disposed)
        {
            _key?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
#pragma warning restore CS1591
}
