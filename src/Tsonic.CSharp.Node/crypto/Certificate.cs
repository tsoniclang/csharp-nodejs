using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Tsonic.CSharp.Node;

/// <summary>
/// SPKAC is a Certificate Signing Request mechanism originally implemented by Netscape.
/// </summary>
public static class Certificate
{
    /// <summary>
    /// Exports the challenge component of an SPKAC data structure.
    /// </summary>
    /// <param name="spkac">The SPKAC data structure.</param>
    /// <returns>The challenge component.</returns>
    public static byte[] exportChallenge(string spkac)
    {
        var spkacBytes = Convert.FromBase64String(spkac);
        return exportChallenge(spkacBytes);
    }

    /// <summary>
    /// Exports the challenge component of an SPKAC data structure.
    /// </summary>
    /// <param name="spkac">The SPKAC data structure.</param>
    /// <returns>The challenge component.</returns>
    public static byte[] exportChallenge(byte[] spkac)
    {
        // SPKAC is not well supported in .NET
        // This would require parsing the ASN.1 structure
        throw new NotImplementedException("SPKAC challenge export is not yet implemented");
    }

    /// <summary>
    /// Exports the public key component of an SPKAC data structure.
    /// </summary>
    /// <param name="spkac">The SPKAC data structure.</param>
    /// <returns>The public key component.</returns>
    public static byte[] exportPublicKey(string spkac)
    {
        var spkacBytes = Convert.FromBase64String(spkac);
        return exportPublicKey(spkacBytes);
    }

    /// <summary>
    /// Exports the public key component of an SPKAC data structure.
    /// </summary>
    /// <param name="spkac">The SPKAC data structure.</param>
    /// <returns>The public key component.</returns>
    public static byte[] exportPublicKey(byte[] spkac)
    {
        // SPKAC is not well supported in .NET
        // This would require parsing the ASN.1 structure
        throw new NotImplementedException("SPKAC public key export is not yet implemented");
    }

    /// <summary>
    /// Validates an SPKAC data structure.
    /// </summary>
    /// <param name="spkac">The SPKAC data structure.</param>
    /// <returns>True if the structure is valid, false otherwise.</returns>
    public static bool verifySpkac(string spkac)
    {
        var spkacBytes = Convert.FromBase64String(spkac);
        return verifySpkac(spkacBytes);
    }

    /// <summary>
    /// Validates an SPKAC data structure.
    /// </summary>
    /// <param name="spkac">The SPKAC data structure.</param>
    /// <returns>True if the structure is valid, false otherwise.</returns>
    public static bool verifySpkac(byte[] spkac)
    {
        // SPKAC is not well supported in .NET
        // This would require parsing the ASN.1 structure and verifying the signature
        throw new NotImplementedException("SPKAC verification is not yet implemented");
    }
}

/// <summary>
/// X509Certificate utility methods for Node.js compatibility.
/// </summary>
public static class X509CertificateExtensions
{
    /// <summary>
    /// Parses an X.509 certificate and returns information about it.
    /// </summary>
    /// <param name="certificate">The certificate in PEM or DER format.</param>
    /// <returns>An object containing certificate information.</returns>
    public static X509CertificateInfo ParseCertificate(string certificate)
    {
        var cert = X509CertificateLoader.LoadCertificate(Encoding.UTF8.GetBytes(certificate));
        return new X509CertificateInfo(cert);
    }

    /// <summary>
    /// Parses an X.509 certificate and returns information about it.
    /// </summary>
    /// <param name="certificate">The certificate in DER format.</param>
    /// <returns>An object containing certificate information.</returns>
    public static X509CertificateInfo ParseCertificate(byte[] certificate)
    {
        var cert = X509CertificateLoader.LoadCertificate(certificate);
        return new X509CertificateInfo(cert);
    }
}

/// <summary>
/// Information about an X.509 certificate.
/// </summary>
public class X509CertificateInfo
{
    private readonly X509Certificate2 _cert;

    internal X509CertificateInfo(X509Certificate2 cert)
    {
        _cert = cert ?? throw new ArgumentNullException(nameof(cert));
    }

    /// <summary>
    /// The certificate's subject.
    /// </summary>
    public string subject => _cert.Subject;

    /// <summary>
    /// The certificate's issuer.
    /// </summary>
    public string issuer => _cert.Issuer;

    /// <summary>
    /// The certificate's serial number.
    /// </summary>
    public string serialNumber => _cert.SerialNumber;

    /// <summary>
    /// The date and time before which the certificate is not valid.
    /// </summary>
    public DateTime validFrom => _cert.NotBefore;

    /// <summary>
    /// The date and time after which the certificate is not valid.
    /// </summary>
    public DateTime validTo => _cert.NotAfter;

    /// <summary>
    /// The certificate's fingerprint (SHA-1 hash).
    /// </summary>
    public string fingerprint => _cert.Thumbprint;

    /// <summary>
    /// The certificate's fingerprint using SHA-256.
    /// </summary>
    public string fingerprint256
    {
        get
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(_cert.RawData);
            return BitConverter.ToString(hash).Replace("-", ":");
        }
    }

    /// <summary>
    /// The certificate's fingerprint using SHA-512.
    /// </summary>
    public string fingerprint512
    {
        get
        {
            using var sha512 = SHA512.Create();
            var hash = sha512.ComputeHash(_cert.RawData);
            return BitConverter.ToString(hash).Replace("-", ":");
        }
    }

    /// <summary>
    /// The public key.
    /// </summary>
    public byte[] publicKey => _cert.PublicKey.EncodedKeyValue.RawData;

    /// <summary>
    /// The raw certificate data in DER format.
    /// </summary>
    public byte[] raw => _cert.RawData;

    /// <summary>
    /// Checks whether the certificate matches the given hostname.
    /// </summary>
    /// <param name="hostname">The hostname to check.</param>
    /// <returns>The matched hostname or null.</returns>
    public string? checkHost(string hostname)
    {
        // Check if the certificate's subject or subject alternative names match the hostname
        // This is a simplified implementation
        var subjectCN = GetCommonName(_cert.Subject);
        if (MatchesHostname(subjectCN, hostname))
        {
            return hostname;
        }

        // Check Subject Alternative Names
        foreach (var extension in _cert.Extensions)
        {
            if (extension is X509SubjectAlternativeNameExtension sanExtension)
            {
                // Parse SAN extension (simplified)
                // Full implementation would parse the ASN.1 structure
                var sanValue = extension.Format(false);
                if (sanValue.Contains(hostname))
                {
                    return hostname;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Checks whether the certificate matches the given email address.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <returns>The matched email or null.</returns>
    public string? checkEmail(string email)
    {
        // Check Subject Alternative Names for email addresses
        foreach (var extension in _cert.Extensions)
        {
            if (extension is X509SubjectAlternativeNameExtension)
            {
                var sanValue = extension.Format(false);
                if (sanValue.Contains(email))
                {
                    return email;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Checks whether the certificate matches the given IP address.
    /// </summary>
    /// <param name="ip">The IP address to check.</param>
    /// <returns>The matched IP or null.</returns>
    public string? checkIP(string ip)
    {
        // Check Subject Alternative Names for IP addresses
        foreach (var extension in _cert.Extensions)
        {
            if (extension is X509SubjectAlternativeNameExtension)
            {
                var sanValue = extension.Format(false);
                if (sanValue.Contains(ip))
                {
                    return ip;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Checks whether this certificate issued the other certificate.
    /// </summary>
    /// <param name="otherCert">The other certificate to check.</param>
    /// <returns>An error message if invalid, or null if valid.</returns>
    public string? checkIssued(X509CertificateInfo otherCert)
    {
        // Check if this certificate issued the other certificate
        if (_cert.Subject != otherCert._cert.Issuer)
        {
            return "Issuer mismatch";
        }

        // Verify signature (simplified - would need full chain validation)
        return null;
    }

    /// <summary>
    /// Checks whether the certificate was issued by the given issuer certificate.
    /// </summary>
    /// <param name="issuerCert">The issuer certificate.</param>
    /// <returns>True if issued by the given issuer.</returns>
    public bool verify(X509CertificateInfo issuerCert)
    {
        try
        {
            using var chain = new X509Chain();
            chain.ChainPolicy.ExtraStore.Add(issuerCert._cert);
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            return chain.Build(_cert);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Returns the PEM-encoded certificate.
    /// </summary>
    /// <returns>The PEM-encoded certificate.</returns>
    public string toPEM()
    {
        return _cert.ExportCertificatePem();
    }

    /// <summary>
    /// Returns a string representation of the certificate.
    /// </summary>
    /// <returns>A string representation.</returns>
    public override string ToString()
    {
        return _cert.ToString();
    }

    private static string GetCommonName(string subject)
    {
        var parts = subject.Split(',');
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            if (trimmed.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
            {
                return trimmed.Substring(3);
            }
        }
        return string.Empty;
    }

    private static bool MatchesHostname(string pattern, string hostname)
    {
        if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(hostname))
            return false;

        // Handle wildcard certificates (*.example.com)
        if (pattern.StartsWith("*."))
        {
            var domain = pattern.Substring(2);
            return hostname.EndsWith(domain, StringComparison.OrdinalIgnoreCase) ||
                   hostname.Equals(domain, StringComparison.OrdinalIgnoreCase);
        }

        return pattern.Equals(hostname, StringComparison.OrdinalIgnoreCase);
    }
}
