using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class isIPv4Tests
{
    [Fact]
    public void isIPv4_ValidIPv4_ReturnsTrue()
    {
        Assert.True(net.isIPv4("127.0.0.1"));
        Assert.True(net.isIPv4("192.168.1.1"));
    }

    [Fact]
    public void isIPv4_IPv6_ReturnsFalse()
    {
        Assert.False(net.isIPv4("::1"));
        Assert.False(net.isIPv4("2001:4860:4860::8888"));
    }

    [Fact]
    public void isIPv4_Invalid_ReturnsFalse()
    {
        Assert.False(net.isIPv4("invalid"));
        Assert.False(net.isIPv4(""));
    }
}
