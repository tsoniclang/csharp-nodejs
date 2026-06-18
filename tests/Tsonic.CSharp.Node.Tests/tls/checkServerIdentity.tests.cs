using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class checkServerIdentityTests
{
    [Fact]
    public void checkServerIdentity_MatchingHostname_ReturnsNull()
    {
        var cert = new PeerCertificate
        {
            subject = new TLSCertificateInfo { CN = "example.com" }
        };
        var error = tls.checkServerIdentity("example.com", cert);
        Assert.Null(error);
    }

    [Fact]
    public void checkServerIdentity_MismatchedHostname_ReturnsError()
    {
        var cert = new PeerCertificate
        {
            subject = new TLSCertificateInfo { CN = "example.com" }
        };
        var error = tls.checkServerIdentity("other.com", cert);
        Assert.NotNull(error);
    }
}
