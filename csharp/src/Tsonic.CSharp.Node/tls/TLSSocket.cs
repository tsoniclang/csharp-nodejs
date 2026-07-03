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
        System.Security.Cryptography.X509Certificates.X509Certificate? certificate,
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

}

#pragma warning restore CS8981
#pragma warning restore IDE1006
