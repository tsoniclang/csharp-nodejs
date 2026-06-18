using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.OpenSsl;
using System.IO;
using System.Text;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Represents an EdDSA public key.
/// </summary>
public class EdDSAPublicKeyObject : KeyObject
{
    private readonly AsymmetricKeyParameter _publicKey;
    private readonly string _keyType;
    private bool _disposed = false;

    internal EdDSAPublicKeyObject(AsymmetricKeyParameter publicKey, string keyType)
    {
        _publicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
        _keyType = keyType ?? throw new ArgumentNullException(nameof(keyType));
    }

    /// <summary>
    /// Gets the underlying BouncyCastle key for internal use.
    /// </summary>
    internal AsymmetricKeyParameter GetKey()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EdDSAPublicKeyObject));
        return _publicKey;
    }

#pragma warning disable CS1591
    public override string type => "public";
    public override string? asymmetricKeyType => _keyType;
    public override int? symmetricKeySize => null;
#pragma warning restore CS1591

    /// <summary>
    /// Exports the public key in PEM or DER format.
    /// </summary>
    public override object export(object? options = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EdDSAPublicKeyObject));

        // Export to SubjectPublicKeyInfo format (PEM)
        var publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(_publicKey);
        using var stringWriter = new StringWriter();
        var pemWriter = new PemWriter(stringWriter);
        pemWriter.WriteObject(publicKeyInfo);
        pemWriter.Writer.Flush();
        return stringWriter.ToString();
    }

#pragma warning disable CS1591
    public override void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
#pragma warning restore CS1591
}

/// <summary>
/// Represents an EdDSA private key.
/// </summary>
public class EdDSAPrivateKeyObject : KeyObject
{
    private readonly AsymmetricKeyParameter _privateKey;
    private readonly string _keyType;
    private bool _disposed = false;

    internal EdDSAPrivateKeyObject(AsymmetricKeyParameter privateKey, string keyType)
    {
        _privateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
        _keyType = keyType ?? throw new ArgumentNullException(nameof(keyType));
    }

    /// <summary>
    /// Gets the underlying BouncyCastle key for internal use.
    /// </summary>
    internal AsymmetricKeyParameter GetKey()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EdDSAPrivateKeyObject));
        return _privateKey;
    }

#pragma warning disable CS1591
    public override string type => "private";
    public override string? asymmetricKeyType => _keyType;
    public override int? symmetricKeySize => null;
#pragma warning restore CS1591

    /// <summary>
    /// Exports the private key in PEM or DER format.
    /// </summary>
    public override object export(object? options = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(EdDSAPrivateKeyObject));

        // Export to PKCS8 format (PEM)
        var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(_privateKey);
        using var stringWriter = new StringWriter();
        var pemWriter = new PemWriter(stringWriter);
        pemWriter.WriteObject(privateKeyInfo);
        pemWriter.Writer.Flush();
        return stringWriter.ToString();
    }

#pragma warning disable CS1591
    public override void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
#pragma warning restore CS1591
}
