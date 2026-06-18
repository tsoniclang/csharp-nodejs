using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class ListenOptionsTests
{
    [Fact]
    public void ListenOptions_AllProperties_CanBeSet()
    {
        var opts = new ListenOptions
        {
            port = 8080,
            host = "localhost",
            path = "/tmp/socket",
            backlog = 511,
            ipv6Only = false
        };

        Assert.Equal(8080, opts.port);
        Assert.Equal("localhost", opts.host);
        Assert.Equal("/tmp/socket", opts.path);
        Assert.Equal(511, opts.backlog);
        Assert.False(opts.ipv6Only);
    }
}
