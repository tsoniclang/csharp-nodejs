using System;
using Tsonic.CSharp.Runtime;
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
            ca = TsValue.from("ca"),
            cert = TsValue.from("cert"),
            key = TsValue.from("key"),
            passphrase = "pass"
        };

        Assert.True(opts.isServer);
        Assert.Equal("example.com", opts.servername);
        Assert.Equal("cert", TsValue.CastDynamic<string>(opts.cert));
    }
}
