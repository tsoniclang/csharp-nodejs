using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Tsonic.CSharp.Runtime;

namespace Tsonic.CSharp.Node.Tests;

internal static class TlsTestCertificate
{
    public static X509Certificate2 Create(string subjectName = "CN=localhost")
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            subjectName,
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, false));
        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                false));
        request.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                new OidCollection
                {
                    new("1.3.6.1.5.5.7.3.1"),
                    new("1.3.6.1.5.5.7.3.2"),
                },
                false));
        var certificate = request.CreateSelfSigned(
            DateTimeOffset.Now.AddDays(-1),
            DateTimeOffset.Now.AddYears(1));
        var pfx = certificate.Export(X509ContentType.Pfx, string.Empty);
        certificate.Dispose();
        return X509CertificateLoader.LoadPkcs12(
            pfx,
            string.Empty,
            X509KeyStorageFlags.Exportable);
    }

    public static TsValue PfxValue(X509Certificate2 certificate) =>
        TsValue.from(Buffer.from(certificate.Export(X509ContentType.Pfx, string.Empty)));
}
