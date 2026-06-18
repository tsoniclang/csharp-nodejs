using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class randomFillTests
{
    [Fact]
    public void randomFill_Async_Works()
    {
        var buffer = new byte[32];
        byte[]? result = null;
        Exception? error = null;

        crypto.randomFill(buffer, 0, 32, (err, buf) =>
        {
            error = err;
            result = buf;
        });

        Assert.Null(error);
        Assert.NotNull(result);
    }

    [Fact]
    public void randomFill_Works()
    {
        var buffer = new byte[32];
        byte[]? result = null;
        Exception? error = null;

        crypto.randomFill(buffer, 0, 16, (err, buf) =>
        {
            error = err;
            result = buf;
        });

        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Same(buffer, result);
    }
}
