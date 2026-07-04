using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class SocketConstructorOptsTests
{
    [Fact]
    public void SocketConstructorOpts_AllProperties_CanBeSet()
    {
        var opts = new SocketConstructorOpts
        {
            fd = 123,
            allowHalfOpen = true,
            readable = true,
            writable = true
        };

        Assert.Equal(123, opts.fd);
        Assert.True(opts.allowHalfOpen);
        Assert.True(opts.readable);
        Assert.True(opts.writable);
    }
}
