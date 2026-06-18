using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class LookupOptionsTests
{
    [Fact]
    public void LookupOptions_AllProperties_CanBeSet()
    {
        var options = new LookupOptions
        {
            family = 4,
            hints = dns.ADDRCONFIG,
            all = true,
            order = "ipv4first",
            verbatim = false
        };

        Assert.Equal(4, options.family);
        Assert.Equal(dns.ADDRCONFIG, options.hints);
        Assert.True(options.all);
        Assert.Equal("ipv4first", options.order);
        Assert.False(options.verbatim);
    }
}
