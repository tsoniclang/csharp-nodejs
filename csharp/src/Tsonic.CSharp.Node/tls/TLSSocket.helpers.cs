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
