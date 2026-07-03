using System;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.OpenSsl;
using System.IO;

namespace Tsonic.CSharp.Node;

/// <summary>
/// Represents a DSA public key.
/// </summary>
public class DSAPublicKeyObject : KeyObject
{
    private readonly AsymmetricKeyParameter _publicKey;
    private bool _disposed = false;

    internal DSAPublicKeyObject(AsymmetricKeyParameter publicKey)
    {
        _publicKey = publicKey ?? throw new ArgumentNullException(nameof(publicKey));
    }

    /// <summary>
    /// Gets the underlying BouncyCastle key for internal use.
    /// </summary>
    internal AsymmetricKeyParameter GetKey()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DSAPublicKeyObject));
        return _publicKey;
    }

#pragma warning disable CS1591
    public override string type => "public";
    public override string? asymmetricKeyType => "dsa";
    public override int? symmetricKeySize => null;
#pragma warning restore CS1591

    /// <summary>
    /// Exports the public key in PEM or DER format.
    /// </summary>
    public override object export(object? options = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DSAPublicKeyObject));

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
/// Represents a DSA private key.
/// </summary>
public class DSAPrivateKeyObject : KeyObject
{
    private readonly AsymmetricKeyParameter _privateKey;
    private bool _disposed = false;

    internal DSAPrivateKeyObject(AsymmetricKeyParameter privateKey)
    {
        _privateKey = privateKey ?? throw new ArgumentNullException(nameof(privateKey));
    }

    /// <summary>
    /// Gets the underlying BouncyCastle key for internal use.
    /// </summary>
    internal AsymmetricKeyParameter GetKey()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DSAPrivateKeyObject));
        return _privateKey;
    }

#pragma warning disable CS1591
    public override string type => "private";
    public override string? asymmetricKeyType => "dsa";
    public override int? symmetricKeySize => null;
#pragma warning restore CS1591

    /// <summary>
    /// Exports the private key in PEM or DER format.
    /// </summary>
    public override object export(object? options = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(DSAPrivateKeyObject));

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
