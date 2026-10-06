using System;
using Microsoft.AspNetCore.Http;
using Tsonic.CSharp.Node.Http;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class NativeHttpLifecycleTests
{
    [Fact]
    public void AbortFailure_LeavesPhysicalCleanupAndNativeTerminalErrorIntact()
    {
        var message = new IncomingMessage(new DefaultHttpContext().Request);
        message.push(Buffer.from("pending body"));
        var original = new InvalidOperationException("original abort callback");
        var supplied = new Exception("native abort");
        var aborts = 0;
        var errors = 0;
        var closes = 0;
        message.once("aborted", () =>
        {
            aborts++;
            throw original;
        });
        message.on("error", (Exception received) =>
        {
            Assert.Same(supplied, received);
            errors++;
        });
        message.once("close", () => closes++);
        Assert.Same(original, Assert.Throws<InvalidOperationException>(() => message.destroy(supplied)));
        Assert.True(message.aborted);
        Assert.True(message.destroyed);
        Assert.False(message.readable);
        Assert.Equal(0, message.readableLength);
        Assert.Equal(0, errors);
        Assert.Equal(0, closes);
        message.destroy();
        Assert.Equal(1, errors);
        Assert.Equal(1, closes);
        message.destroy();
        Assert.Equal(1, aborts);
        Assert.Equal(1, errors);
        Assert.Equal(1, closes);
    }
}
