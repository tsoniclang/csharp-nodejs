using System;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation
#pragma warning disable SYSLIB0039 // Obsolete TLS protocol versions

/// <summary>
/// Accepts encrypted connections using TLS or SSL.
/// </summary>
public class TLSServer : Server
{
    private SecureContext? _secureContext;
    private TlsOptions? _options;
    private byte[]? _ticketKeys;

    /// <summary>
    /// Creates a new TLS Server instance.
    /// </summary>
    public TLSServer() : this(null, null)
    {
    }

    /// <summary>
    /// Creates a new TLS Server instance with a secure connection listener.
    /// </summary>
    public TLSServer(Action<TLSSocket>? secureConnectionListener) : this(null, secureConnectionListener)
    {
    }

    /// <summary>
    /// Creates a new TLS Server instance with options and a secure connection listener.
    /// </summary>
    public TLSServer(TlsOptions? options, Action<TLSSocket>? secureConnectionListener) : base()
    {
        _options = options;

        if (options != null)
        {
            // Create secure context from options
            _secureContext = tls.createSecureContext(new SecureContextOptions
            {
                key = options.key,
                cert = options.cert,
                ca = options.ca,
                passphrase = options.passphrase,
                minVersion = options.rejectUnauthorized == true ? "TLSv1.2" : null
            });
        }

        if (secureConnectionListener != null)
        {
            on("secureConnection", secureConnectionListener);
        }

        // Listen for regular connections and wrap them with TLS
        on("connection", (Action<Socket>)OnConnection);
    }

    private void OnConnection(Socket socket)
    {
        // Wrap the TCP socket with TLS
        Task.Run(async () =>
        {
            try
            {
                var tcpClient = socket.GetTcpClient();
                if (tcpClient == null || !tcpClient.Connected)
                    return;

                var networkStream = tcpClient.GetStream();
                var sslStream = new SslStream(
                    networkStream,
                    leaveInnerStreamOpen: false,
                    userCertificateValidationCallback: ValidateClientCertificate
                );

                // Perform server-side TLS handshake
                var serverCertificate = _secureContext?.Certificate;
                if (serverCertificate == null)
                {
                    emit("tlsClientError", new Exception("Server certificate not configured"), socket);
                    socket.destroy();
                    return;
                }

                var protocols = _secureContext?.Protocols ?? (SslProtocols.Tls12 | SslProtocols.Tls13);
                var requestClientCert = _options?.requestCert ?? false;

                await sslStream.AuthenticateAsServerAsync(
                    serverCertificate,
                    clientCertificateRequired: requestClientCert,
                    enabledSslProtocols: protocols,
                    checkCertificateRevocation: false
                );

                // Create TLSSocket from the connected socket
                var tlsSocket = CreateTLSSocket(socket, sslStream);

                // Emit secureConnection event
                emit("secureConnection", tlsSocket);
            }
            catch (Exception ex)
            {
                emit("tlsClientError", ex, socket);
                socket.destroy();
            }
        });
    }

    private TLSSocket CreateTLSSocket(Socket baseSocket, SslStream sslStream)
    {
        var tlsSocket = new TLSSocket(baseSocket, new TLSSocketOptions
        {
            isServer = true,
            secureContext = _secureContext
        });

        // Inject the authenticated SslStream
        tlsSocket.SetSslStream(sslStream);

        return tlsSocket;
    }

    private bool ValidateClientCertificate(
        object sender,
        X509Certificate? certificate,
        X509Chain? chain,
        SslPolicyErrors sslPolicyErrors)
    {
        // If client cert not required, accept
        if (_options?.requestCert != true)
            return true;

        // If client cert required but reject unauthorized is false, accept
        if (_options?.rejectUnauthorized != true)
            return true;

        // Validate client certificate
        return sslPolicyErrors == SslPolicyErrors.None;
    }

    /// <summary>
    /// Adds a secure context that will be used if the client request's SNI name matches the supplied hostname.
    /// </summary>
    public void addContext(string hostname, object context)
    {
        // SNI context switching not fully implemented
        // Would require mapping hostname to SecureContext
    }

    /// <summary>
    /// Returns the session ticket keys.
    /// </summary>
    public byte[] getTicketKeys()
    {
        if (_ticketKeys == null)
        {
            _ticketKeys = new byte[48];
            new Random().NextBytes(_ticketKeys);
        }
        return _ticketKeys;
    }

    /// <summary>
    /// Replaces the secure context of an existing server.
    /// </summary>
    public void setSecureContext(SecureContextOptions options)
    {
        _secureContext = tls.createSecureContext(options);
    }

    /// <summary>
    /// Sets the session ticket keys.
    /// </summary>
    public void setTicketKeys(byte[] keys)
    {
        if (keys.Length != 48)
        {
            throw new ArgumentException("Ticket keys must be 48 bytes");
        }
        _ticketKeys = keys;
    }

    internal SecureContext? GetSecureContext()
    {
        return _secureContext;
    }

    internal TlsOptions? GetOptions()
    {
        return _options;
    }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
