using System;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// Certificate fields.
/// </summary>
public class TLSCertificateInfo
{
    /// <summary>
    /// Country code.
    /// </summary>
    public string C { get; set; } = string.Empty;

    /// <summary>
    /// State or province.
    /// </summary>
    public string ST { get; set; } = string.Empty;

    /// <summary>
    /// Locality.
    /// </summary>
    public string L { get; set; } = string.Empty;

    /// <summary>
    /// Organization.
    /// </summary>
    public string O { get; set; } = string.Empty;

    /// <summary>
    /// Organizational unit.
    /// </summary>
    public string OU { get; set; } = string.Empty;

    /// <summary>
    /// Common name.
    /// </summary>
    public string CN { get; set; } = string.Empty;
}

/// <summary>
/// Peer certificate information.
/// </summary>
public class PeerCertificate
{
    /// <summary>
    /// True if a Certificate Authority (CA), false otherwise.
    /// </summary>
    public bool ca { get; set; }

    /// <summary>
    /// The DER encoded X.509 certificate data.
    /// </summary>
    public byte[]? raw { get; set; }

    /// <summary>
    /// The certificate subject.
    /// </summary>
    public TLSCertificateInfo subject { get; set; } = new TLSCertificateInfo();

    /// <summary>
    /// The certificate issuer.
    /// </summary>
    public TLSCertificateInfo issuer { get; set; } = new TLSCertificateInfo();

    /// <summary>
    /// The date-time the certificate is valid from.
    /// </summary>
    public string valid_from { get; set; } = string.Empty;

    /// <summary>
    /// The date-time the certificate is valid to.
    /// </summary>
    public string valid_to { get; set; } = string.Empty;

    /// <summary>
    /// The certificate serial number, as a hex string.
    /// </summary>
    public string serialNumber { get; set; } = string.Empty;

    /// <summary>
    /// The SHA-1 digest of the DER encoded certificate.
    /// </summary>
    public string fingerprint { get; set; } = string.Empty;

    /// <summary>
    /// The SHA-256 digest of the DER encoded certificate.
    /// </summary>
    public string fingerprint256 { get; set; } = string.Empty;

    /// <summary>
    /// The SHA-512 digest of the DER encoded certificate.
    /// </summary>
    public string fingerprint512 { get; set; } = string.Empty;

    /// <summary>
    /// The extended key usage.
    /// </summary>
    public string[]? ext_key_usage { get; set; }

    /// <summary>
    /// Subject alternative names.
    /// </summary>
    public string? subjectaltname { get; set; }
}

/// <summary>
/// Detailed peer certificate with issuer chain.
/// </summary>
public class DetailedPeerCertificate : PeerCertificate
{
    /// <summary>
    /// The issuer certificate object.
    /// </summary>
    public DetailedPeerCertificate? issuerCertificate { get; set; }
}

/// <summary>
/// Cipher name and protocol information.
/// </summary>
public class CipherNameAndProtocol
{
    /// <summary>
    /// The cipher name.
    /// </summary>
    public string name { get; set; } = string.Empty;

    /// <summary>
    /// SSL/TLS protocol version.
    /// </summary>
    public string version { get; set; } = string.Empty;

    /// <summary>
    /// IETF name for the cipher suite.
    /// </summary>
    public string standardName { get; set; } = string.Empty;
}

/// <summary>
/// Ephemeral key exchange information.
/// </summary>
public class EphemeralKeyInfo
{
    /// <summary>
    /// The type: 'DH' or 'ECDH'.
    /// </summary>
    public string type { get; set; } = string.Empty;

    /// <summary>
    /// The name (available only when type is 'ECDH').
    /// </summary>
    public string? name { get; set; }

    /// <summary>
    /// The size of parameter of an ephemeral key exchange.
    /// </summary>
    public int size { get; set; }
}

/// <summary>
/// Secure context options for TLS configuration.
/// </summary>
public class SecureContextOptions
{
    /// <summary>
    /// Optionally override the trusted CA certificates.
    /// </summary>
    public object? ca { get; set; }

    /// <summary>
    /// Cert chains in PEM format.
    /// </summary>
    public object? cert { get; set; }

    /// <summary>
    /// Cipher suite specification.
    /// </summary>
    public string? ciphers { get; set; }

    /// <summary>
    /// Private keys in PEM format.
    /// </summary>
    public object? key { get; set; }

    /// <summary>
    /// Shared passphrase for private key and/or PFX.
    /// </summary>
    public string? passphrase { get; set; }

    /// <summary>
    /// PFX or PKCS12 encoded private key and certificate chain.
    /// </summary>
    public object? pfx { get; set; }

    /// <summary>
    /// Maximum TLS version to allow.
    /// </summary>
    public string? maxVersion { get; set; }

    /// <summary>
    /// Minimum TLS version to allow.
    /// </summary>
    public string? minVersion { get; set; }
}

/// <summary>
/// Common connection options for TLS.
/// </summary>
public class CommonConnectionOptions
{
    /// <summary>
    /// An optional TLS context object.
    /// </summary>
    public SecureContext? secureContext { get; set; }

    /// <summary>
    /// Enable TLS packet trace information.
    /// </summary>
    public bool? enableTrace { get; set; }

    /// <summary>
    /// Request a certificate from clients.
    /// </summary>
    public bool? requestCert { get; set; }

    /// <summary>
    /// Reject unauthorized connections.
    /// </summary>
    public bool? rejectUnauthorized { get; set; }

    /// <summary>
    /// ALPN protocols.
    /// </summary>
    public string[]? ALPNProtocols { get; set; }
}

/// <summary>
/// TLS socket options.
/// </summary>
public class TLSSocketOptions : CommonConnectionOptions
{
    /// <summary>
    /// If true the TLS socket will be instantiated in server-mode.
    /// </summary>
    public bool? isServer { get; set; }

    /// <summary>
    /// An optional Server instance.
    /// </summary>
    public Server? server { get; set; }

    /// <summary>
    /// Server name for SNI.
    /// </summary>
    public string? servername { get; set; }

    /// <summary>
    /// CA certificates.
    /// </summary>
    public object? ca { get; set; }

    /// <summary>
    /// Certificate chain.
    /// </summary>
    public object? cert { get; set; }

    /// <summary>
    /// Private key.
    /// </summary>
    public object? key { get; set; }

    /// <summary>
    /// Passphrase.
    /// </summary>
    public string? passphrase { get; set; }
}

/// <summary>
/// Connection options for TLS client.
/// </summary>
public class ConnectionOptions : CommonConnectionOptions
{
    /// <summary>
    /// Host name.
    /// </summary>
    public string? host { get; set; }

    /// <summary>
    /// Port number.
    /// </summary>
    public int? port { get; set; }

    /// <summary>
    /// Server name for SNI.
    /// </summary>
    public string? servername { get; set; }

    /// <summary>
    /// CA certificates.
    /// </summary>
    public object? ca { get; set; }

    /// <summary>
    /// Certificate chain.
    /// </summary>
    public object? cert { get; set; }

    /// <summary>
    /// Private key.
    /// </summary>
    public object? key { get; set; }

    /// <summary>
    /// Passphrase.
    /// </summary>
    public string? passphrase { get; set; }

    /// <summary>
    /// Timeout in milliseconds.
    /// </summary>
    public int? timeout { get; set; }
}

/// <summary>
/// TLS server options.
/// </summary>
public class TlsOptions : CommonConnectionOptions
{
    /// <summary>
    /// Handshake timeout in milliseconds.
    /// </summary>
    public int? handshakeTimeout { get; set; }

    /// <summary>
    /// Session timeout in seconds.
    /// </summary>
    public int? sessionTimeout { get; set; }

    /// <summary>
    /// CA certificates.
    /// </summary>
    public object? ca { get; set; }

    /// <summary>
    /// Certificate chain.
    /// </summary>
    public object? cert { get; set; }

    /// <summary>
    /// Private key.
    /// </summary>
    public object? key { get; set; }

    /// <summary>
    /// Passphrase.
    /// </summary>
    public string? passphrase { get; set; }

    /// <summary>
    /// Allow half-open TCP connections.
    /// </summary>
    public bool? allowHalfOpen { get; set; }

    /// <summary>
    /// Pause on connect.
    /// </summary>
    public bool? pauseOnConnect { get; set; }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
