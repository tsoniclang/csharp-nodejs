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
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResponseRetiredTimeoutDestruction_PublishesOneCloseAndPreservesTheNativeError(bool withError)
    {
        var closes = 0;
        var errors = 0;
        var timeouts = 0;
        var finishes = 0;
        var expected = withError ? new InvalidOperationException("native response timeout") : null;
        JsEventLoop.Run(async () =>
        {
            var context = new DefaultHttpContext();
            var response = new ServerResponse(context.Response, JsEventLoop.Capture());
            response.on("close", () => closes++);
            response.on("finish", () => finishes++);
            response.on<Exception>("error", actual =>
            {
                Assert.Same(expected, actual);
                errors++;
            });
            response.setTimeout(20, () =>
            {
                timeouts++;
                response.destroy(expected);
            });
            System.Threading.Thread.Sleep(80);
            response.end();
            await response.Completion;
            Assert.True(response.Completion.IsCompletedSuccessfully);
            Assert.True(response.destroyed);
            response.destroy();
        });
        Assert.Equal(1, closes);
        Assert.Equal(1, timeouts);
        Assert.Equal(withError ? 1 : 0, errors);
        Assert.Equal(0, finishes);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void IncomingTimeoutDestruction_PublishesOneCloseAndNativeRelease(bool completeBody, bool withError)
    {
        var closes = 0;
        var errors = 0;
        var timeouts = 0;
        var releases = 0;
        var expected = withError ? new InvalidOperationException("native incoming timeout") : null;
        JsEventLoop.Run(() =>
        {
            var response = new System.Net.Http.HttpResponseMessage
            {
                Content = new System.Net.Http.ByteArrayContent(System.Array.Empty<byte>()),
            };
            var message = new IncomingMessage(response, System.Threading.CancellationToken.None,
                () => releases++, JsEventLoop.Capture());
            message.on("close", () => closes++);
            message.on<Exception>("error", actual =>
            {
                Assert.Same(expected, actual);
                errors++;
            });
            message.setTimeout(20, () =>
            {
                timeouts++;
                message.destroy(expected);
            });
            System.Threading.Thread.Sleep(80);
            if (completeBody)
                message.StartClientBody();
            else
                message.CloseAfterResponse();
        });
        Assert.Equal(1, closes);
        Assert.Equal(1, releases);
        Assert.Equal(1, timeouts);
        Assert.Equal(withError ? 1 : 0, errors);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ResponseElapsedTimeout_IsDeliveredBeforeNativeCompletionButCancelledByDestruction(bool destroy)
    {
        var timeouts = 0;
        var closes = 0;
        JsEventLoop.Run(async () =>
        {
            var context = new DefaultHttpContext();
            var response = new ServerResponse(context.Response, JsEventLoop.Capture());
            response.on("close", () =>
            {
                Assert.Equal(destroy ? 0 : 1, timeouts);
                closes++;
            });
            response.setTimeout(20, () =>
            {
                Assert.False(response.destroyed);
                timeouts++;
            });
            System.Threading.Thread.Sleep(80);
            if (destroy)
                response.destroy();
            else
            {
                response.end();
                await response.Completion;
            }
        });
        Assert.Equal(destroy ? 0 : 1, timeouts);
        Assert.Equal(1, closes);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ElapsedTimeout_IsDeliveredBeforeNativeCompletionButCancelledByDestruction(bool destroy)
    {
        var connection = new DefaultConnectionContext();
        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = connection.ConnectionClosed.Register(
            static state => ((TaskCompletionSource)state!).TrySetResult(), closed);
        var admitted = false;
        var timeouts = 0;
        var closes = 0;
        try
        {
            JsEventLoop.Run(() =>
            {
                var context = new DefaultHttpContext();
                context.Features.Set<IConnectionLifetimeFeature>(
                    connection.Features.GetRequiredFeature<IConnectionLifetimeFeature>());
                var message = new IncomingMessage(context.Request, JsEventLoop.Capture());
                admitted = true;
                message.on("close", () =>
                {
                    Assert.Equal(destroy ? 0 : 1, timeouts);
                    closes++;
                });
                message.setTimeout(20, () =>
                {
                    Assert.False(message.destroyed);
                    timeouts++;
                });
                System.Threading.Thread.Sleep(80);
                if (destroy)
                    message.destroy();
                else
                    message.CloseAfterResponse();
            });
            Assert.Equal(destroy ? 0 : 1, timeouts);
            Assert.Equal(1, closes);
        }
        finally
        {
            if (admitted)
                await closed.Task;
            registration.Dispose();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task AbortFailure_LeavesPhysicalCleanupAndNativeTerminalErrorIntact()
    {
        var connection = new DefaultConnectionContext();
        var closed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = connection.ConnectionClosed.Register(static state => ((TaskCompletionSource)state!).TrySetResult(), closed);
        var admitted = false;
        try
        {
            JsEventLoop.Run(() =>
            {
                var context = new DefaultHttpContext();
                context.Features.Set<IConnectionLifetimeFeature>(connection.Features.GetRequiredFeature<IConnectionLifetimeFeature>());
                var message = new IncomingMessage(context.Request, Tsonic.CSharp.Js.JsEventLoop.Capture());
                admitted = true;
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
        finally
        {
            if (admitted)
            {
                await closed.Task;
                Assert.True(connection.ConnectionClosed.IsCancellationRequested);
            }
            registration.Dispose();
            await connection.DisposeAsync();
        }
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
