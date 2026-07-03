using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Xunit;

#pragma warning disable SYSLIB0057 // Obsolete X509Certificate2 constructor

namespace Tsonic.CSharp.Node.Tests;

public class createSecureContextTests : IDisposable
{
    private X509Certificate2? _serverCert;

    public createSecureContextTests()
    {
        _serverCert = GenerateSelfSignedCertificate("CN=localhost");
    }

    public void Dispose()
    {
        _serverCert?.Dispose();
    }

    private static X509Certificate2 GenerateSelfSignedCertificate(string subjectName)
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, false));

        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                false));

        request.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection {
                    new Oid("1.3.6.1.5.5.7.3.1"),
                    new Oid("1.3.6.1.5.5.7.3.2")
                },
                false));

        var certificate = request.CreateSelfSigned(
            DateTimeOffset.Now.AddDays(-1),
            DateTimeOffset.Now.AddYears(1));

        var pfxData = certificate.Export(X509ContentType.Pfx, "");
        return new X509Certificate2(pfxData, "", X509KeyStorageFlags.Exportable);
    }

    [Fact]
    public void createSecureContext_NoOptions_ReturnsContext()
    {
        var context = tls.createSecureContext();
        Assert.NotNull(context);
    }

    [Fact]
    public void createSecureContext_WithCert_LoadsCertificate()
    {
        var context = tls.createSecureContext(new SecureContextOptions
        {
            cert = _serverCert
        });
        Assert.NotNull(context);
        Assert.NotNull(context.Certificate);
    }
}
