using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class isIPv6Tests
{
    [Fact]
    public void isIPv6_ValidIPv6_ReturnsTrue()
    {
        Assert.True(net.isIPv6("::1"));
        Assert.True(net.isIPv6("2001:4860:4860::8888"));
    }

    [Fact]
    public void isIPv6_IPv4_ReturnsFalse()
    {
        Assert.False(net.isIPv6("127.0.0.1"));
        Assert.False(net.isIPv6("192.168.1.1"));
    }

    [Fact]
    public void isIPv6_Invalid_ReturnsFalse()
    {
        Assert.False(net.isIPv6("invalid"));
        Assert.False(net.isIPv6(""));
    }
}
