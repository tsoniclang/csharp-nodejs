using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class isIPTests
{
    [Fact]
    public void isIP_ValidIPv4_Returns4()
    {
        Assert.Equal(4, net.isIP("127.0.0.1"));
        Assert.Equal(4, net.isIP("192.168.1.1"));
        Assert.Equal(4, net.isIP("8.8.8.8"));
    }

    [Fact]
    public void isIP_ValidIPv6_Returns6()
    {
        Assert.Equal(6, net.isIP("::1"));
        Assert.Equal(6, net.isIP("2001:4860:4860::8888"));
        Assert.Equal(6, net.isIP("fe80::1"));
    }

    [Fact]
    public void isIP_InvalidIP_Returns0()
    {
        Assert.Equal(0, net.isIP("invalid"));
        Assert.Equal(0, net.isIP("999.999.999.999"));
        Assert.Equal(0, net.isIP(""));
        Assert.Equal(0, net.isIP("localhost"));
    }
}
