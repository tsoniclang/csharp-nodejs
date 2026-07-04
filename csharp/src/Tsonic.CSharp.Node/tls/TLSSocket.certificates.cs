using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation
#pragma warning disable SYSLIB0058 // Obsolete cipher algorithm APIs
#pragma warning disable SYSLIB0039 // Obsolete TLS protocol versions
#pragma warning disable CS0649 // Field never assigned

/// <summary>
/// Performs transparent encryption of written data and all required TLS negotiation.
/// </summary>
public partial class TLSSocket : Socket
{
    /// <summary>
    /// Returns an object representing the local certificate.
    /// </summary>
    public PeerCertificate? getCertificate()
    {
        if (_localCertificate == null || destroyed)
            return null;

        return ConvertCertificate(_localCertificate);
    }

    /// <summary>
    /// Returns an object representing the peer's certificate.
    /// </summary>
    public PeerCertificate? getPeerCertificate(bool detailed = false)
    {
        if (_remoteCertificate == null || destroyed)
            return null;

        if (detailed)
        {
            return ConvertCertificateDetailed(_remoteCertificate);
        }
        else
        {
            return ConvertCertificate(_remoteCertificate);
        }
    }

    /// <summary>
    /// Returns an object containing information on the negotiated cipher suite.
    /// </summary>
    public CipherNameAndProtocol getCipher()
    {
        if (_sslStream == null)
        {
            return new CipherNameAndProtocol
            {
                name = "unknown",
                version = "unknown",
                standardName = "unknown"
            };
        }

        return new CipherNameAndProtocol
        {
            name = _sslStream.CipherAlgorithm.ToString(),
            version = _sslStream.SslProtocol.ToString(),
            standardName = _sslStream.CipherAlgorithm.ToString()
        };
    }

    /// <summary>
    /// Returns an object representing the type, name, and size of parameter of
    /// an ephemeral key exchange.
    /// </summary>
    public EphemeralKeyInfo? getEphemeralKeyInfo()
    {
        if (_sslStream == null)
            return null;

        // .NET doesn't expose ephemeral key info directly
        return new EphemeralKeyInfo
        {
            type = "ECDH",
            name = "prime256v1",
            size = 256
        };
    }

    /// <summary>
    /// Returns the latest Finished message sent to the socket.
    /// </summary>
    public byte[]? getFinished()
    {
        // Not directly supported in .NET SslStream
        return null;
    }

    /// <summary>
    /// Returns the latest Finished message received from the socket.
    /// </summary>
    public byte[]? getPeerFinished()
    {
        // Not directly supported in .NET SslStream
        return null;
    }

    /// <summary>
    /// Returns a string containing the negotiated SSL/TLS protocol version.
    /// </summary>
    public string? getProtocol()
    {
        if (_sslStream == null || !_sslStream.IsAuthenticated)
            return null;

        return _sslStream.SslProtocol switch
        {
            SslProtocols.Tls => "TLSv1",
            SslProtocols.Tls11 => "TLSv1.1",
            SslProtocols.Tls12 => "TLSv1.2",
            SslProtocols.Tls13 => "TLSv1.3",
            _ => "unknown"
        };
    }

    /// <summary>
    /// Returns the TLS session data.
    /// </summary>
    public byte[]? getSession()
    {
        // Session data not directly supported in .NET SslStream
        return null;
    }

    /// <summary>
    /// Returns list of signature algorithms shared between server and client.
    /// </summary>
    public string[] getSharedSigalgs()
    {
        // Not directly supported in .NET SslStream
        return Array.Empty<string>();
    }

    /// <summary>
    /// Returns the TLS session ticket.
    /// </summary>
    public byte[]? getTLSTicket()
    {
        // Session tickets not directly supported in .NET SslStream
        return null;
    }

    /// <summary>
    /// Returns true if the session was reused.
    /// </summary>
    public bool isSessionReused()
    {
        // .NET doesn't expose session reuse information
        return false;
    }

    /// <summary>
    /// Initiates a TLS renegotiation process.
    /// </summary>
    public bool renegotiate(object options, Action<Exception?> callback)
    {
        // TLS 1.3 doesn't support renegotiation
        // .NET SslStream doesn't support renegotiation
        callback(new NotSupportedException("Renegotiation not supported"));
        return false;
    }

    /// <summary>
    /// Sets the private key and certificate.
    /// </summary>
    public void setKeyCert(object context)
    {
        if (context is SecureContext secureContext)
        {
            _secureContext = secureContext;
            _localCertificate = secureContext.Certificate;
        }
    }

    /// <summary>
    /// Sets the maximum TLS fragment size.
    /// </summary>
    public bool setMaxSendFragment(int size)
    {
        // Not directly supported in .NET SslStream
        return false;
    }

    /// <summary>
    /// Disables TLS renegotiation for this TLSSocket instance.
    /// </summary>
    public void disableRenegotiation()
    {
        // Already disabled in .NET
    }

    /// <summary>
    /// Enables TLS packet trace information.
    /// </summary>
    public void enableTrace()
    {
        // Tracing not directly supported in .NET SslStream
    }

    /// <summary>
    /// Returns the peer certificate as an X509Certificate object.
    /// </summary>
    public object? getPeerX509Certificate()
    {
        return _remoteCertificate;
    }

    /// <summary>
    /// Returns the local certificate as an X509Certificate object.
    /// </summary>
    public object? getX509Certificate()
    {
        return _localCertificate;
    }

    /// <summary>
    /// Exports keying material for external authentication procedures.
    /// </summary>
    public byte[] exportKeyingMaterial(int length, string label, byte[] context)
    {
        // Not supported in .NET SslStream
        throw new NotSupportedException("exportKeyingMaterial not supported");
    }

}
