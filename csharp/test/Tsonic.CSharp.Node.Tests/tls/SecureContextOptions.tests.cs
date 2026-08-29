using System;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class SecureContextOptionsTests
{
    [Fact]
    public void SecureContextOptions_AllProperties_CanBeSet()
    {
        var opts = new SecureContextOptions
        {
            ca = TsValue.from("ca-cert"),
            cert = TsValue.from("cert"),
            key = TsValue.from("key"),
            passphrase = "pass",
            ciphers = "HIGH",
            maxVersion = "TLSv1.3",
            minVersion = "TLSv1.2"
        };

        Assert.Equal("ca-cert", TsValue.CastDynamic<string>(opts.ca));
        Assert.Equal("pass", opts.passphrase);
    }
}
