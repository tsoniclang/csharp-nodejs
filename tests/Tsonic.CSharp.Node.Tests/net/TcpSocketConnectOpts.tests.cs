using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class TcpSocketConnectOptsTests
{
    [Fact]
    public void TcpSocketConnectOpts_AllProperties_CanBeSet()
    {
        var opts = new TcpSocketConnectOpts
        {
            port = 8080,
            host = "localhost",
            localAddress = "127.0.0.1",
            localPort = 9090,
            hints = 1,
            family = 4,
            noDelay = true,
            keepAlive = true,
            keepAliveInitialDelay = 1000
        };

        Assert.Equal(8080, opts.port);
        Assert.Equal("localhost", opts.host);
        Assert.Equal("127.0.0.1", opts.localAddress);
        Assert.Equal(9090, opts.localPort);
        Assert.Equal(1, opts.hints);
        Assert.Equal(4, opts.family);
        Assert.True(opts.noDelay);
        Assert.True(opts.keepAlive);
        Assert.Equal(1000, opts.keepAliveInitialDelay);
    }
}
