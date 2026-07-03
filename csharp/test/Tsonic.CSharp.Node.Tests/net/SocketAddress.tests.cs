using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class SocketAddressTests
{
    [Fact]
    public void SocketAddress_Constructor_CreatesInstance()
    {
        var options = new SocketAddressInitOptions
        {
            address = "127.0.0.1",
            family = "ipv4",
            port = 8080
        };

        var socketAddress = new SocketAddress(options);
        Assert.NotNull(socketAddress);
        Assert.Equal("127.0.0.1", socketAddress.address);
        Assert.Equal("ipv4", socketAddress.family);
        Assert.Equal(8080, socketAddress.port);
    }

    [Fact]
    public void SocketAddress_Constructor_DefaultValues()
    {
        var options = new SocketAddressInitOptions();
        var socketAddress = new SocketAddress(options);

        Assert.Equal("0.0.0.0", socketAddress.address);
        Assert.Equal("ipv4", socketAddress.family);
        Assert.Equal(0, socketAddress.port);
    }

    [Fact]
    public void SocketAddress_Flowlabel_CanBeSet()
    {
        var options = new SocketAddressInitOptions
        {
            address = "::1",
            family = "ipv6",
            port = 8080,
            flowlabel = 12345
        };

        var socketAddress = new SocketAddress(options);
        Assert.Equal(12345, socketAddress.flowlabel);
    }
}
