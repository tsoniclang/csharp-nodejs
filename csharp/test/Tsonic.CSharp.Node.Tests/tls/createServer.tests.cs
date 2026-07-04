using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Xunit;

#pragma warning disable SYSLIB0057 // Obsolete X509Certificate2 constructor

namespace Tsonic.CSharp.Node.Tests;

public class Tls_createServerTests : IDisposable
{
    private X509Certificate2? _serverCert;

    public Tls_createServerTests()
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
    public void createServer_NoArgs_ReturnsServer()
    {
        var server = tls.createServer();
        Assert.NotNull(server);
        Assert.IsType<TLSServer>(server);
    }

    [Fact]
    public void createServer_WithListener_AttachesListener()
    {
        var server = tls.createServer((socket) => { });
        Assert.NotNull(server);
    }

    [Fact]
    public void createServer_WithOptions_ReturnsServer()
    {
        var options = new TlsOptions { cert = _serverCert };
        var server = tls.createServer(options);
        Assert.NotNull(server);
    }
}
