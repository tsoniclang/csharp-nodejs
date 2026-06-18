using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class TlsOptionsTests
{
    [Fact]
    public void TlsOptions_AllProperties_CanBeSet()
    {
        var opts = new TlsOptions
        {
            handshakeTimeout = 120000,
            sessionTimeout = 300,
            ca = "ca",
            cert = "cert",
            key = "key",
            passphrase = "pass"
        };

        Assert.Equal(120000, opts.handshakeTimeout);
        Assert.Equal(300, opts.sessionTimeout);
    }
}
