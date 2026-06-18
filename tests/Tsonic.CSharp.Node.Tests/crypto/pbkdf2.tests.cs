using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class pbkdf2Tests
{
    [Fact]
    public void pbkdf2_Async_Works()
    {
        byte[]? result = null;
        Exception? error = null;

        crypto.pbkdf2("password", "salt", 1000, 32, "sha256", (err, key) =>
        {
            error = err;
            result = key;
        });

        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact]
    public void pbkdf2_Callback_Works()
    {
        byte[]? result = null;
        Exception? error = null;

        crypto.pbkdf2("password", "salt", 1000, 32, "sha256", (err, key) =>
        {
            error = err;
            result = key;
        });

        Assert.Null(error);
        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }
}
