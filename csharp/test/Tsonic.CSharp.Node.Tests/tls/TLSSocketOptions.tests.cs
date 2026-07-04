using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class TLSSocketOptionsTests
{
    [Fact]
    public void TLSSocketOptions_AllProperties_CanBeSet()
    {
        var opts = new TLSSocketOptions
        {
            isServer = true,
            servername = "example.com",
            ca = "ca",
            cert = "cert",
            key = "key",
            passphrase = "pass"
        };

        Assert.True(opts.isServer);
        Assert.Equal("example.com", opts.servername);
    }
}
