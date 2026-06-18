using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class timingSafeEqualTests
{
    [Fact]
    public void timingSafeEqual_ReturnsTrueForEqualBuffers()
    {
        var buffer1 = Encoding.UTF8.GetBytes("Hello");
        var buffer2 = Encoding.UTF8.GetBytes("Hello");

        Assert.True(crypto.timingSafeEqual(buffer1, buffer2));
    }

    [Fact]
    public void timingSafeEqual_ReturnsFalseForDifferentBuffers()
    {
        var buffer1 = Encoding.UTF8.GetBytes("Hello");
        var buffer2 = Encoding.UTF8.GetBytes("World");

        Assert.False(crypto.timingSafeEqual(buffer1, buffer2));
    }

    [Fact]
    public void timingSafeEqual_ReturnsFalseForDifferentLengths()
    {
        var buffer1 = Encoding.UTF8.GetBytes("Hello");
        var buffer2 = Encoding.UTF8.GetBytes("Hello!");

        Assert.False(crypto.timingSafeEqual(buffer1, buffer2));
    }

    [Fact]
    public void timingSafeEqual_WithIdenticalContent()
    {
        var buffer1 = new byte[] { 1, 2, 3, 4, 5 };
        var buffer2 = new byte[] { 1, 2, 3, 4, 5 };

        Assert.True(crypto.timingSafeEqual(buffer1, buffer2));
    }
}
