using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Xunit;

#pragma warning disable SYSLIB0057 // Obsolete X509Certificate2 constructor

namespace Tsonic.CSharp.Node.Tests;

public class TLSServerTests : IDisposable
{
    private X509Certificate2? _serverCert;

    public TLSServerTests()
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
    public void TLSServer_Constructor_CreatesInstance()
    {
        var server = new TLSServer();
        Assert.NotNull(server);
    }

    [Fact]
    public void TLSServer_ConstructorWithListener_AttachesListener()
    {
        var server = new TLSServer((socket) => { });
        Assert.NotNull(server);
    }

    [Fact]
    public void TLSServer_ConstructorWithOptions_CreatesInstance()
    {
        var options = new TlsOptions { cert = _serverCert };
        var server = new TLSServer(options, null);
        Assert.NotNull(server);
    }

    [Fact]
    public void TLSServer_GetTicketKeys_Returns48Bytes()
    {
        var server = new TLSServer();
        var keys = server.getTicketKeys();
        Assert.NotNull(keys);
        Assert.Equal(48, keys.Length);
    }

    [Fact]
    public void TLSServer_SetTicketKeys_AcceptsValidKeys()
    {
        var server = new TLSServer();
        var keys = new byte[48];
        new Random().NextBytes(keys);

        var exception = Record.Exception(() => server.setTicketKeys(keys));
        Assert.Null(exception);
    }

    [Fact]
    public void TLSServer_SetTicketKeys_InvalidLength_Throws()
    {
        var server = new TLSServer();
        var keys = new byte[32]; // Wrong length

        Assert.Throws<ArgumentException>(() => server.setTicketKeys(keys));
    }

    [Fact]
    public void TLSServer_SetSecureContext_AcceptsOptions()
    {
        var server = new TLSServer();
        var exception = Record.Exception(() =>
            server.setSecureContext(new SecureContextOptions { cert = _serverCert }));
        Assert.Null(exception);
    }

    [Fact]
    public void TLSServer_AddContext_DoesNotThrow()
    {
        var server = new TLSServer();
        var exception = Record.Exception(() =>
            server.addContext("example.com", new SecureContextOptions()));
        Assert.Null(exception);
    }
}
