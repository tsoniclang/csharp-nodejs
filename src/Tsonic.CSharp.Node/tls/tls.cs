using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Tsonic.CSharp.Node;

#pragma warning disable CS8981 // Lowercase type names
#pragma warning disable IDE1006 // Naming rule violation

/// <summary>
/// The tls module provides an implementation of the Transport Layer Security (TLS) and
/// Secure Socket Layer (SSL) protocols.
/// </summary>
public static class tls
{
    /// <summary>
    /// Client renegotiation limit.
    /// </summary>
    public static readonly int CLIENT_RENEG_LIMIT = 3;

    /// <summary>
    /// Client renegotiation window in seconds.
    /// </summary>
    public static readonly int CLIENT_RENEG_WINDOW = 600;

    /// <summary>
    /// The default curve name to use for ECDH key agreement.
    /// </summary>
    public static string DEFAULT_ECDH_CURVE = "auto";

    /// <summary>
    /// The default value of the maxVersion option.
    /// </summary>
    public static string DEFAULT_MAX_VERSION = "TLSv1.3";

    /// <summary>
    /// The default value of the minVersion option.
    /// </summary>
    public static string DEFAULT_MIN_VERSION = "TLSv1.2";

    /// <summary>
    /// The default value of the ciphers option.
    /// </summary>
    public static string DEFAULT_CIPHERS = "TLS_AES_256_GCM_SHA384:TLS_CHACHA20_POLY1305_SHA256:TLS_AES_128_GCM_SHA256";

    /// <summary>
    /// Root certificates from the bundled Mozilla CA store.
    /// </summary>
    public static readonly string[] rootCertificates = Array.Empty<string>();

    /// <summary>
    /// Creates a new TLS server.
    /// </summary>
    public static TLSServer createServer(Action<TLSSocket>? secureConnectionListener = null)
    {
        return new TLSServer(secureConnectionListener);
    }

    /// <summary>
    /// Creates a new TLS server with options.
    /// </summary>
    public static TLSServer createServer(TlsOptions options, Action<TLSSocket>? secureConnectionListener = null)
    {
        return new TLSServer(options, secureConnectionListener);
    }

    /// <summary>
    /// Creates a TLS connection to a server.
    /// </summary>
    public static TLSSocket connect(ConnectionOptions options, Action? secureConnectListener = null)
    {
        var socket = new Socket();

        // Create secure context from options
        var secureContext = createSecureContext(new SecureContextOptions
        {
            ca = options.ca,
            cert = options.cert,
            key = options.key,
            passphrase = options.passphrase,
            minVersion = options.rejectUnauthorized == false ? null : "TLSv1.2"
        });

        var tlsSocket = new TLSSocket(socket, new TLSSocketOptions
        {
            isServer = false,
            servername = options.servername ?? options.host,
            ca = options.ca,
            cert = options.cert,
            key = options.key,
            passphrase = options.passphrase,
            rejectUnauthorized = options.rejectUnauthorized,
            secureContext = secureContext
        });

        if (secureConnectListener != null)
        {
            tlsSocket.once("secureConnect", secureConnectListener);
        }

        // Connect to the server - TLS handshake happens in TLSSocket constructor
        var port = options.port ?? 443;
        var host = options.host ?? "localhost";
        socket.connect(port, host);

        return tlsSocket;
    }

    /// <summary>
    /// Creates a TLS connection to a server.
    /// </summary>
    public static TLSSocket connect(int port, string? host = null, ConnectionOptions? options = null, Action? secureConnectListener = null)
    {
        var opts = options ?? new ConnectionOptions();
        opts.port = port;
        opts.host = host ?? "localhost";
        return connect(opts, secureConnectListener);
    }

    /// <summary>
    /// Creates a TLS connection to a server.
    /// </summary>
    public static TLSSocket connect(int port, ConnectionOptions? options = null, Action? secureConnectListener = null)
    {
        var opts = options ?? new ConnectionOptions();
        opts.port = port;
        return connect(opts, secureConnectListener);
    }

    /// <summary>
    /// Creates a secure context.
    /// </summary>
    public static SecureContext createSecureContext(SecureContextOptions? options = null)
    {
        var context = new SecureContext();

        if (options != null)
        {
            // Load certificate and key
            context.LoadCertificate(options.cert, options.key, options.passphrase);

            // Load CA certificates
            context.LoadCACertificates(options.ca);

            // Set protocol versions
            context.SetProtocols(options.minVersion, options.maxVersion);
        }

        return context;
    }

    /// <summary>
    /// Verifies the certificate is issued to hostname.
    /// </summary>
    public static Exception? checkServerIdentity(string hostname, PeerCertificate cert)
    {
        if (cert == null)
        {
            return new Exception("Certificate is required");
        }

        // Check CN matches hostname
        if (cert.subject.CN == hostname)
        {
            return null;
        }

        // Check subject alternative names
        if (cert.subjectaltname != null)
        {
            var sans = cert.subjectaltname.Split(',').Select(s => s.Trim()).ToArray();
            foreach (var san in sans)
            {
                if (san.StartsWith("DNS:"))
                {
                    var dnsName = san.Substring(4);
                    if (dnsName == hostname || MatchesWildcard(dnsName, hostname))
                    {
                        return null;
                    }
                }
                else if (san.StartsWith("IP Address:"))
                {
                    var ipAddress = san.Substring(11);
                    if (ipAddress == hostname)
                    {
                        return null;
                    }
                }
            }
        }

        return new Exception($"Hostname/IP does not match certificate's altnames: Host: {hostname}. is not in the cert's altnames");
    }

    /// <summary>
    /// Returns an array with the names of the supported TLS ciphers.
    /// </summary>
    public static string[] getCiphers()
    {
        // .NET doesn't expose the exact list of supported ciphers
        // Return a common list
        return new[]
        {
            "TLS_AES_256_GCM_SHA384",
            "TLS_CHACHA20_POLY1305_SHA256",
            "TLS_AES_128_GCM_SHA256",
            "TLS_ECDHE_RSA_WITH_AES_256_GCM_SHA384",
            "TLS_ECDHE_RSA_WITH_AES_128_GCM_SHA256",
            "TLS_DHE_RSA_WITH_AES_256_GCM_SHA384",
            "TLS_DHE_RSA_WITH_AES_128_GCM_SHA256"
        };
    }

    /// <summary>
    /// Returns an array containing the CA certificates.
    /// </summary>
    public static string[] getCACertificates(string type = "default")
    {
        // .NET uses the system's trusted CA store by default
        // We can return certificates from the system store
        var certificates = new List<string>();

        try
        {
            using var store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            foreach (var cert in store.Certificates)
            {
                try
                {
                    var pem = ExportToPem(cert);
                    if (!string.IsNullOrEmpty(pem))
                    {
                        certificates.Add(pem);
                    }
                }
                catch
                {
                    // Skip certificates that can't be exported
                }
            }
        }
        catch
        {
            // Return empty if we can't access the store
        }

        return certificates.ToArray();
    }

    /// <summary>
    /// Sets the default CA certificates.
    /// </summary>
    public static void setDefaultCACertificates(string[] certs)
    {
        // .NET doesn't support changing the default CA store at runtime
        // This would require custom certificate validation
    }

    private static bool MatchesWildcard(string pattern, string hostname)
    {
        if (!pattern.StartsWith("*."))
            return false;

        var suffix = pattern.Substring(2);
        return hostname.EndsWith(suffix) && hostname.IndexOf('.') == hostname.LastIndexOf('.');
    }

    private static string ExportToPem(X509Certificate2 cert)
    {
        try
        {
            var base64 = Convert.ToBase64String(cert.RawData);
            var pem = new System.Text.StringBuilder();
            pem.AppendLine("-----BEGIN CERTIFICATE-----");

            for (int i = 0; i < base64.Length; i += 64)
            {
                pem.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
            }

            pem.AppendLine("-----END CERTIFICATE-----");
            return pem.ToString();
        }
        catch
        {
            return string.Empty;
        }
    }
}

#pragma warning restore CS8981
#pragma warning restore IDE1006
