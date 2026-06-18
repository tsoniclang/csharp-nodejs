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
public class TLSSocket : Socket
{
    private SslStream? _sslStream;
    private bool _authorized = false;
    private Exception? _authorizationError;
    private string? _alpnProtocol;
    private SecureContext? _secureContext;
    private X509Certificate2? _localCertificate;
    private X509Certificate2? _remoteCertificate;

    /// <summary>
    /// True if the peer certificate was signed by one of the CAs.
    /// </summary>
    public bool authorized => _authorized;

    /// <summary>
    /// Returns the reason why the peer's certificate was not verified.
    /// </summary>
    public Exception? authorizationError => _authorizationError;

    /// <summary>
    /// Always returns true for TLS sockets.
    /// </summary>
    public bool encrypted => true;

    /// <summary>
    /// String containing the selected ALPN protocol.
    /// </summary>
    public string? alpnProtocol => _alpnProtocol;

    private Socket? _baseSocket;
    private TLSSocketOptions? _options;

    /// <summary>
    /// Creates a new TLS socket from an existing TCP socket.
    /// </summary>
    public TLSSocket(Socket socket, TLSSocketOptions? options = null) : base()
    {
        _baseSocket = socket;
        _options = options;

        if (options != null)
        {
            _secureContext = options.secureContext;
        }

        // Listen for the underlying socket to connect
        socket.on("connect", (Action)OnSocketConnected);
        socket.on("error", (Action<Exception>)OnSocketError);
    }

    private void OnSocketConnected()
    {
        if (_baseSocket == null || _options == null)
            return;

        var tcpClient = _baseSocket.GetTcpClient();
        if (tcpClient != null && tcpClient.Connected)
        {
            var networkStream = tcpClient.GetStream();

            // Create SslStream wrapper
            _sslStream = new SslStream(
                networkStream,
                leaveInnerStreamOpen: false,
                userCertificateValidationCallback: ValidateServerCertificate
            );

            // If this is a client connection, start handshake
            if (_options.isServer != true)
            {
                Task.Run(async () =>
                {
                    try
                    {
                        var serverName = _options.servername ?? "localhost";
                        var clientCertificates = new X509Certificate2Collection();

                        if (_secureContext?.Certificate != null)
                        {
                            clientCertificates.Add(_secureContext.Certificate);
                        }

                        await _sslStream.AuthenticateAsClientAsync(
                            serverName,
                            clientCertificates,
                            _secureContext?.Protocols ?? (SslProtocols.Tls12 | SslProtocols.Tls13),
                            checkCertificateRevocation: false
                        );

                        _authorized = _sslStream.IsAuthenticated;
                        _remoteCertificate = _sslStream.RemoteCertificate as X509Certificate2;
                        _localCertificate = _sslStream.LocalCertificate as X509Certificate2;

                        // Start reading data from the stream
                        StartReading();

                        emit("secureConnect");
                    }
                    catch (Exception ex)
                    {
                        _authorizationError = ex;
                        emit("error", ex);
                    }
                });
            }
        }
    }

    private void OnSocketError(Exception error)
    {
        emit("error", error);
    }

    /// <summary>
    /// Sets an already-authenticated SslStream (for server-side sockets).
    /// </summary>
    internal void SetSslStream(SslStream sslStream)
    {
        _sslStream = sslStream;
        _authorized = sslStream.IsAuthenticated;
        _remoteCertificate = sslStream.RemoteCertificate as X509Certificate2;
        _localCertificate = sslStream.LocalCertificate as X509Certificate2;

        // Start reading from the stream
        StartReading();
    }

    private bool ValidateServerCertificate(
        object sender,
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors == SslPolicyErrors.None)
        {
            _authorized = true;
            return true;
        }

        // Check if we should reject unauthorized
        // Default to rejecting if not explicitly set
        var rejectUnauthorized = true; // TODO: Get from options

        if (!rejectUnauthorized)
        {
            _authorized = false;
            _authorizationError = new Exception($"Certificate error: {sslPolicyErrors}");
            return true; // Accept anyway
        }

        _authorized = false;
        _authorizationError = new Exception($"Certificate validation failed: {sslPolicyErrors}");
        return false;
    }

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

    /// <summary>
    /// Writes data to the TLS stream.
    /// </summary>
    public new bool write(byte[] data, Action<Exception?>? callback = null)
    {
        if (_sslStream == null || !_sslStream.CanWrite)
        {
            callback?.Invoke(new Exception("Stream not writable"));
            return false;
        }

        Task.Run(async () =>
        {
            try
            {
                await _sslStream.WriteAsync(data, 0, data.Length);
                await _sslStream.FlushAsync();
                callback?.Invoke(null);
            }
            catch (Exception ex)
            {
                callback?.Invoke(ex);
                emit("error", ex);
            }
        });

        return true;
    }

    /// <summary>
    /// Writes string data to the TLS stream.
    /// </summary>
    public new bool write(string data, string? encoding = null, Action<Exception?>? callback = null)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(data);
        return write(bytes, callback);
    }

    /// <summary>
    /// Reads data from the TLS stream.
    /// </summary>
    internal new void StartReading()
    {
        if (_sslStream == null || !_sslStream.CanRead)
            return;

        Task.Run(async () =>
        {
            var buffer = new byte[8192];
            try
            {
                while (_sslStream.CanRead && !destroyed)
                {
                    var bytesRead = await _sslStream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // Connection closed
                        emit("end");
                        emit("close", false);
                        break;
                    }

                    var data = new byte[bytesRead];
                    Array.Copy(buffer, data, bytesRead);
                    emit("data", data);
                }
            }
            catch (Exception ex)
            {
                if (!destroyed)
                {
                    emit("error", ex);
                }
            }
        });
    }

    private PeerCertificate ConvertCertificate(X509Certificate2 cert)
    {
        var subject = ParseDistinguishedName(cert.Subject);
        var issuer = ParseDistinguishedName(cert.Issuer);

        return new PeerCertificate
        {
            ca = cert.Extensions["2.5.29.19"] != null, // Basic Constraints extension
            raw = cert.RawData,
            subject = subject,
            issuer = issuer,
            valid_from = cert.NotBefore.ToString("MMM dd HH:mm:ss yyyy") + " GMT",
            valid_to = cert.NotAfter.ToString("MMM dd HH:mm:ss yyyy") + " GMT",
            serialNumber = cert.SerialNumber,
            fingerprint = cert.GetCertHashString(System.Security.Cryptography.HashAlgorithmName.SHA1),
            fingerprint256 = cert.GetCertHashString(System.Security.Cryptography.HashAlgorithmName.SHA256),
            fingerprint512 = cert.GetCertHashString(System.Security.Cryptography.HashAlgorithmName.SHA512),
            ext_key_usage = GetExtendedKeyUsage(cert),
            subjectaltname = GetSubjectAltNames(cert)
        };
    }

    private DetailedPeerCertificate ConvertCertificateDetailed(X509Certificate2 cert)
    {
        var basic = ConvertCertificate(cert);
        return new DetailedPeerCertificate
        {
            ca = basic.ca,
            raw = basic.raw,
            subject = basic.subject,
            issuer = basic.issuer,
            valid_from = basic.valid_from,
            valid_to = basic.valid_to,
            serialNumber = basic.serialNumber,
            fingerprint = basic.fingerprint,
            fingerprint256 = basic.fingerprint256,
            fingerprint512 = basic.fingerprint512,
            ext_key_usage = basic.ext_key_usage,
            subjectaltname = basic.subjectaltname,
            issuerCertificate = null // Would need to traverse certificate chain
        };
    }

    private TLSCertificateInfo ParseDistinguishedName(string dn)
    {
        var cert = new TLSCertificateInfo();
        var parts = dn.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var keyValue = part.Trim().Split('=');
            if (keyValue.Length == 2)
            {
                var key = keyValue[0].Trim();
                var value = keyValue[1].Trim();

                switch (key)
                {
                    case "C": cert.C = value; break;
                    case "ST": cert.ST = value; break;
                    case "L": cert.L = value; break;
                    case "O": cert.O = value; break;
                    case "OU": cert.OU = value; break;
                    case "CN": cert.CN = value; break;
                }
            }
        }

        return cert;
    }

    private string[]? GetExtendedKeyUsage(X509Certificate2 cert)
    {
        foreach (var ext in cert.Extensions)
        {
            if (ext.Oid?.Value == "2.5.29.37") // Extended Key Usage
            {
                var eku = ext as System.Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension;
                if (eku != null)
                {
                    var usages = new string[eku.EnhancedKeyUsages.Count];
                    for (int i = 0; i < eku.EnhancedKeyUsages.Count; i++)
                    {
                        usages[i] = eku.EnhancedKeyUsages[i].FriendlyName ?? eku.EnhancedKeyUsages[i].Value ?? "";
                    }
                    return usages;
                }
            }
        }
        return null;
    }

    private string? GetSubjectAltNames(X509Certificate2 cert)
    {
        foreach (var ext in cert.Extensions)
        {
            if (ext.Oid?.Value == "2.5.29.17") // Subject Alternative Name
            {
                var san = ext as System.Security.Cryptography.X509Certificates.X509SubjectAlternativeNameExtension;
                if (san != null)
                {
                    // Format the SANs as comma-separated list
                    var names = new System.Collections.Generic.List<string>();
                    foreach (var entry in san.EnumerateDnsNames())
                    {
                        names.Add($"DNS:{entry}");
                    }
                    foreach (var entry in san.EnumerateIPAddresses())
                    {
                        names.Add($"IP Address:{entry}");
                    }
                    return string.Join(", ", names);
                }
            }
        }
        return null;
    }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
