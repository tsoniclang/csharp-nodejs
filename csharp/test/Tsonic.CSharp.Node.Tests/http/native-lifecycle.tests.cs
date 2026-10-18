using Tsonic.CSharp.Js;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using Tsonic.CSharp.Node.Http;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class NativeHttpLifecycleTests
{
    [Fact]
    public async Task AbortFailure_LeavesPhysicalCleanupAndNativeTerminalErrorIntact()
    {
        await using var connection = new DefaultConnectionContext();
        JsEventLoop.Run(() =>
        {
            var context = new DefaultHttpContext();
            context.Features.Set<IConnectionLifetimeFeature>(connection.Features.GetRequiredFeature<IConnectionLifetimeFeature>());
            var message = new IncomingMessage(context.Request, Tsonic.CSharp.Js.JsEventLoop.Capture());
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
        });
    }

    [Fact]
    public void MissingConnectionLifetimeFeature_IsRejectedWithoutRetainedWork()
    {
        JsEventLoop.Run(() =>
        {
            var context = new DefaultHttpContext();
            var error = Assert.Throws<InvalidOperationException>(() =>
                new IncomingMessage(context.Request, JsEventLoop.Capture()));
            Assert.Contains(nameof(IConnectionLifetimeFeature), error.Message, StringComparison.Ordinal);
            Assert.False(ProcessKeepAlive.HasReferences);
        });
        Assert.False(ProcessKeepAlive.HasReferences);
    }
}
