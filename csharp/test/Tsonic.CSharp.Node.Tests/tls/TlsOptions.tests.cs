using System;
using Tsonic.CSharp.Runtime;
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
            ca = TsValue.from("ca"),
            cert = TsValue.from("cert"),
            key = TsValue.from("key"),
            passphrase = "pass"
        };

        Assert.Equal(120000, opts.handshakeTimeout);
        Assert.Equal(300, opts.sessionTimeout);
        Assert.Equal("ca", TsValue.CastDynamic<string>(opts.ca));
    }
}
