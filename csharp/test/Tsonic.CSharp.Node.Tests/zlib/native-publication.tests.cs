using System;
using System.Threading;
using System.Threading.Tasks;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class NativeCodecPublicationTests
{
    [Fact]
    public void Publication_ReusesAcknowledgementAndPreservesTheOwnedChunk()
    {
        JsEventLoop.Run(async () =>
        {
            var bytes = new byte[] { 1, 2, 3 };
            var received = 0;
            using var cancellation = new CancellationTokenSource();
            using var publication = new CodecPublication(
                value =>
                {
                    Assert.Same(bytes, value);
                    received++;
                    return true;
                },
                error => throw error,
                _ => throw new InvalidOperationException("Accepted output must not wait for capacity."),
                cancellation.Token, JsEventLoop.Capture());
            ProcessKeepAlive.Acquire();
            try
            {
                for (var index = 0; index < 128; index++)
                    await publication.Publish(bytes).AsTask().WaitAsync(TimeSpan.FromSeconds(5));
                Assert.Equal(128, received);
            }
            finally
            {
                ProcessKeepAlive.Release();
            }
        });
    }

    [Fact]
    public void Publication_CancellationSettlesPendingOutputWithoutInvokingThePublisher()
    {
        JsEventLoop.Run(async () =>
        {
            using var cancellation = new CancellationTokenSource();
            using var publication = new CodecPublication(
                _ => throw new InvalidOperationException("Cancelled output was published."),
                error => throw error,
                _ => throw new InvalidOperationException("Cancelled output requested capacity."),
                cancellation.Token, JsEventLoop.Capture());
            var pending = publication.Publish(new byte[1]);
            Assert.False(pending.IsCompleted);
            cancellation.Cancel();
            var failure = await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await pending.AsTask().WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Equal(cancellation.Token, failure.CancellationToken);
            Assert.False(ProcessKeepAlive.HasReferences);
        });
    }

    [Fact]
    public void Publication_RejectsOverlapAndRetainsTheOriginalCompletion()
    {
        JsEventLoop.Run(async () =>
        {
            var calls = 0;
            using var publication = new CodecPublication(
                _ => { calls++; return true; },
                error => throw error,
                _ => Task.CompletedTask,
                CancellationToken.None, JsEventLoop.Capture());
            var pending = publication.Publish(new byte[1]);
            var overlap = publication.Publish(new byte[1]);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await overlap);
            await pending.AsTask().WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(1, calls);
            Assert.False(ProcessKeepAlive.HasReferences);
        });
    }

    [Fact]
    public void Publication_AcknowledgesPressureBeforeAwaitingCapacityAndCancellation()
    {
        JsEventLoop.Run(async () =>
        {
            using var cancellation = new CancellationTokenSource();
            var capacity = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var waiting = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var publication = new CodecPublication(
                _ => false,
                error => throw error,
                token =>
                {
                    Assert.Equal(cancellation.Token, token);
                    waiting.SetResult();
                    return capacity.Task.WaitAsync(token);
                },
                cancellation.Token, JsEventLoop.Capture());
            var pending = publication.Publish(new byte[1]);
            await waiting.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.False(pending.IsCompleted);
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await publication.Publish(new byte[1]));
            cancellation.Cancel();
            var failure = await Assert.ThrowsAsync<TaskCanceledException>(async () =>
                await pending.AsTask().WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Equal(cancellation.Token, failure.CancellationToken);
            Assert.False(ProcessKeepAlive.HasReferences);
        });
    }
}
