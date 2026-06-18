using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class PeerCertificateTests
{
    [Fact]
    public void PeerCertificate_AllProperties_CanBeSet()
    {
        var cert = new PeerCertificate
        {
            ca = true,
            raw = new byte[] { 1, 2, 3 },
            serialNumber = "ABC123",
            fingerprint = "SHA1",
            fingerprint256 = "SHA256",
            fingerprint512 = "SHA512",
            valid_from = "Jan 1 2024",
            valid_to = "Dec 31 2025"
        };

        Assert.True(cert.ca);
        Assert.NotNull(cert.raw);
        Assert.Equal("ABC123", cert.serialNumber);
    }
}
