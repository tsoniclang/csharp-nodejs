using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class SecureContextOptionsTests
{
    [Fact]
    public void SecureContextOptions_AllProperties_CanBeSet()
    {
        var opts = new SecureContextOptions
        {
            ca = "ca-cert",
            cert = "cert",
            key = "key",
            passphrase = "pass",
            ciphers = "HIGH",
            maxVersion = "TLSv1.3",
            minVersion = "TLSv1.2"
        };

        Assert.Equal("ca-cert", opts.ca);
        Assert.Equal("pass", opts.passphrase);
    }
}
