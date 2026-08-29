using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation
#pragma warning disable SYSLIB0057 // Obsolete X509Certificate2 constructor
#pragma warning disable SYSLIB0039 // Obsolete TLS protocol versions

/// <summary>
/// Represents a secure context for TLS connections.
/// </summary>
public class SecureContext
{
    private X509Certificate2? _certificate;
    private X509Certificate2Collection? _caCertificates;
    private SslProtocols _protocols = SslProtocols.None;

    /// <summary>
    /// The server certificate.
    /// </summary>
    public X509Certificate2? Certificate => _certificate;

    /// <summary>
    /// The CA certificates.
    /// </summary>
    public X509Certificate2Collection? CACertificates => _caCertificates;

    /// <summary>
    /// The SSL/TLS protocols.
    /// </summary>
    public SslProtocols Protocols => _protocols;

    /// <summary>
    /// Creates a new secure context.
    /// </summary>
    public SecureContext()
    {
    }

    /// <summary>
    /// Loads a certificate from PEM or PFX data.
    /// </summary>
    internal void LoadCertificate(
        TsValue cert,
        TsValue key,
        TsValue pfx,
        string? passphrase)
    {
        var hasCertificate = !cert.isUndefined();
        var hasKey = !key.isUndefined();
        var hasPfx = !pfx.isUndefined();
        if (hasPfx && (hasCertificate || hasKey))
        {
            throw new ArgumentException(
                "TLS certificate options must select either cert/key PEM material or one pfx carrier.");
        }
        if (hasPfx)
        {
            if (pfx.unwrap() is not Buffer pfxBuffer)
                throw new ArgumentException("PFX input must be a Buffer.", nameof(pfx));
            _certificate = new X509Certificate2(pfxBuffer.InternalData, passphrase);
            return;
        }
        if (!hasCertificate && !hasKey)
            return;
        if (!hasCertificate || !hasKey)
            throw new ArgumentException("A PEM certificate and private key must be supplied together.");

        var certificatePem = PemText(cert, nameof(cert));
        var keyPem = PemText(key, nameof(key));
        _certificate = passphrase == null
            ? X509Certificate2.CreateFromPem(certificatePem, keyPem)
            : X509Certificate2.CreateFromEncryptedPem(certificatePem, keyPem, passphrase);
    }

    /// <summary>
    /// Loads CA certificates.
    /// </summary>
    internal void LoadCACertificates(TsValue ca)
    {
        if (ca.isUndefined())
            return;
        var value = ca.unwrap();

        _caCertificates = new X509Certificate2Collection();

        if (value is IEnumerable<string> certificates)
        {
            foreach (var certificateText in certificates)
            {
                _caCertificates.Add(X509Certificate2.CreateFromPem(certificateText));
            }
            return;
        }
        if (value is string certificatePem)
        {
            _caCertificates.Add(X509Certificate2.CreateFromPem(certificatePem));
            return;
        }
        if (value is Buffer buffer)
        {
            _caCertificates.Add(X509Certificate2.CreateFromPem(Encoding.UTF8.GetString(buffer.InternalData)));
            return;
        }
        throw new ArgumentException("CA input must be PEM text or a string array of PEM certificates.", nameof(ca));
    }

    /// <summary>
    /// Sets the SSL/TLS protocol versions.
    /// </summary>
    public void SetProtocols(string? minVersion, string? maxVersion)
    {
        // Default to TLS 1.2 and 1.3
        _protocols = SslProtocols.Tls12 | SslProtocols.Tls13;

        if (minVersion != null || maxVersion != null)
        {
            _protocols = SslProtocols.None;

            // Build protocol flags based on min/max versions
            var versions = new[] { "TLSv1", "TLSv1.1", "TLSv1.2", "TLSv1.3" };
            var protocols = new[] {
                SslProtocols.Tls,
                SslProtocols.Tls11,
                SslProtocols.Tls12,
                SslProtocols.Tls13
            };

            var minIndex = minVersion != null ? Array.IndexOf(versions, minVersion) : 0;
            var maxIndex = maxVersion != null ? Array.IndexOf(versions, maxVersion) : versions.Length - 1;

            if (minIndex < 0) minIndex = 2; // Default to TLS 1.2
            if (maxIndex < 0) maxIndex = 3; // Default to TLS 1.3

            for (var i = minIndex; i <= maxIndex && i < protocols.Length; i++)
            {
                _protocols |= protocols[i];
            }
        }
    }

    private static string PemText(TsValue value, string parameterName) =>
        value.unwrap() switch
        {
            string text => text,
            Buffer buffer => Encoding.UTF8.GetString(buffer.InternalData),
            _ => throw new ArgumentException("PEM input must be a string or Buffer.", parameterName),
        };
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
