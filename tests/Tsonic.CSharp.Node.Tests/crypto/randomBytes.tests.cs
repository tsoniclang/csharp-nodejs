using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class randomBytesTests
{
    [Fact]
    public void randomBytes_GeneratesCorrectLength()
    {
        var bytes = crypto.randomBytes(32);
        Assert.Equal(32, bytes.Length);
    }

    [Fact]
    public void randomBytes_GeneratesDifferentValues()
    {
        var bytes1 = crypto.randomBytes(16);
        var bytes2 = crypto.randomBytes(16);

        Assert.NotEqual(bytes1, bytes2);
    }

    [Fact]
    public void randomBytes_Async_Works()
    {
        byte[]? result = null;
        Exception? error = null;

        crypto.randomBytes(32, (err, buf) =>
        {
            error = err;
            result = buf;
        });

        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact]
    public void randomBytes_Callback_Works()
    {
        byte[]? result = null;
        Exception? error = null;
        crypto.randomBytes(32, (err, bytes) =>
        {
            error = err;
            result = bytes;
        });

        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }
}
