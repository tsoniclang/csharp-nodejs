using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class ConnectionOptionsTests
{
    [Fact]
    public void ConnectionOptions_AllProperties_CanBeSet()
    {
        var opts = new ConnectionOptions
        {
            host = "example.com",
            port = 443,
            servername = "example.com",
            timeout = 5000
        };

        Assert.Equal("example.com", opts.host);
        Assert.Equal(443, opts.port);
        Assert.Equal(5000, opts.timeout);
    }
}
