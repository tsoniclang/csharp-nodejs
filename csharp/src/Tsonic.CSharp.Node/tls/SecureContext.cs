using System;
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
    /// Internal context reference (for compatibility).
    /// </summary>
    public object? context { get; set; }

    /// <summary>
    /// Creates a new secure context.
    /// </summary>
    public SecureContext()
    {
        context = this;
    }

    /// <summary>
    /// Loads a certificate from PEM or PFX data.
    /// </summary>
    public void LoadCertificate(object? cert, object? key, string? passphrase)
    {
        cert = Unwrap(cert);
        key = Unwrap(key);
        if (cert == null)
            return;

        if (cert is Buffer certBuffer)
            cert = certBuffer.InternalData;
        if (key is Buffer keyBuffer)
            key = Encoding.UTF8.GetString(keyBuffer.InternalData);

        if (cert is X509Certificate2 x509Cert)
        {
            _certificate = x509Cert;
            return;
        }
        if (cert is string certString)
        {
            if (key is not string keyString)
            {
                throw new ArgumentException("A PEM certificate requires a PEM private key.", nameof(key));
            }
            _certificate = passphrase == null
                ? X509Certificate2.CreateFromPem(certString, keyString)
                : X509Certificate2.CreateFromEncryptedPem(certString, keyString, passphrase);
            return;
        }
        if (cert is byte[] certBytes)
        {
            _certificate = new X509Certificate2(certBytes, passphrase);
            return;
        }
        throw new ArgumentException("Certificate input must be PEM text, PKCS#12 bytes, or an X509Certificate2.", nameof(cert));
    }

    /// <summary>
    /// Loads CA certificates.
    /// </summary>
    public void LoadCACertificates(object? ca)
    {
        ca = Unwrap(ca);
        if (ca is Buffer buffer)
            ca = Encoding.UTF8.GetString(buffer.InternalData);
        if (ca == null)
            return;

        _caCertificates = new X509Certificate2Collection();

        if (ca is string[] caArray)
        {
            foreach (var caString in caArray)
            {
                _caCertificates.Add(X509Certificate2.CreateFromPem(caString));
            }
            return;
        }
        if (ca is string caString)
        {
            _caCertificates.Add(X509Certificate2.CreateFromPem(caString));
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

    private static object? Unwrap(object? value) =>
        value is TsValue typed
            ? typed.isUndefined() ? null : typed.unwrap()
            : value;
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
