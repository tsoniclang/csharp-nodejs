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
            ca = TsValue.fromnative("ca-cert"),
            cert = TsValue.fromnative("cert"),
            key = TsValue.fromnative("key"),
            passphrase = "pass",
            ciphers = "HIGH",
            maxVersion = "TLSv1.3",
            minVersion = "TLSv1.2"
        };

        Assert.Equal("ca-cert", TsValue.CastDynamic<string>(opts.ca));
        Assert.Equal("pass", opts.passphrase);
    }
}
