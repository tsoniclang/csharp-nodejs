using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class createServerTests
{
    [Fact]
    public void createServer_NoArgs_ReturnsServer()
    {
        var server = net.createServer();
        Assert.NotNull(server);
        Assert.IsType<Server>(server);
    }

    [Fact]
    public void createServer_WithConnectionListener_AttachesListener()
    {
        var server = net.createServer((socket) =>
        {
            // Listener attached
        });
        Assert.NotNull(server);
        // Listener will be called when a connection is made
    }

    [Fact]
    public void createServer_WithOptions_ReturnsServer()
    {
        var options = new ServerOpts { allowHalfOpen = true };
        var server = net.createServer(options);
        Assert.NotNull(server);
    }
}
