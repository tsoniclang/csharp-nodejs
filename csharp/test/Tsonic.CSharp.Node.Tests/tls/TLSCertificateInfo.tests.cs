using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class TLSCertificateInfoTests
{
    [Fact]
    public void TLSCertificateInfo_AllProperties_CanBeSet()
    {
        var cert = new TLSCertificateInfo
        {
            C = "US",
            ST = "CA",
            L = "San Francisco",
            O = "Test Corp",
            OU = "Test Unit",
            CN = "test.example.com"
        };

        Assert.Equal("US", cert.C);
        Assert.Equal("CA", cert.ST);
        Assert.Equal("San Francisco", cert.L);
        Assert.Equal("Test Corp", cert.O);
        Assert.Equal("Test Unit", cert.OU);
        Assert.Equal("test.example.com", cert.CN);
    }
}
