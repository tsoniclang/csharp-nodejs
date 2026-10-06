using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class NativeStreamLifecycleTests
{
    [Fact]
    public void End_PropagatesTheOriginalFinishCallbackFailure()
    {
        var writable = new Writable();
        var expected = new InvalidOperationException("native finish callback");
        writable.once("finish", () => throw expected);
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => writable.end()));
        Assert.True(writable.writableFinished);
    }

    [Fact]
    public void Uncork_PropagatesTheOriginalDrainCallbackFailure()
    {
        var writable = new SmallWritable();
        var expected = new InvalidOperationException("native drain callback");
        writable.once("drain", () => throw expected);
        writable.cork();
        Assert.False(writable.write(Buffer.from("pressure")));
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => writable.uncork()));
        Assert.False(writable.writableNeedDrain);
        writable.end();
        Assert.True(writable.writableFinished);
    }

    [Fact]
    public void DestructionCallbackFailure_DoesNotPreventCloseOrRepeatTheError()
    {
        var transform = new Transform();
        Readable projection = transform;
        var expected = new InvalidOperationException("native destruction listener");
        var closes = 0;
        var errors = 0;
        projection.on<Exception>("error", _ =>
        {
            errors++;
            throw expected;
        });
        transform.on("close", () => closes++);
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() =>
            transform.destroy(new Exception("native supplied error"))));
        transform.destroy();
        Assert.Equal(1, errors);
        Assert.Equal(1, closes);
        Assert.True(transform.destroyed);
        Assert.False(transform.readable);
        Assert.False(transform.writable);
    }

    [Fact]
    public void Destruction_SettlesPendingTransformAcknowledgementsWithoutAnotherError()
    {
        var transform = new DeferredTransform();
        var expected = new InvalidOperationException("native pending write");
        var errors = 0;
        var closes = 0;
        transform.on<Exception>("error", error =>
        {
            Assert.Same(expected, error);
            errors++;
        });
        transform.on("close", () => closes++);
        transform.write(Buffer.from("pending"));
        transform.destroy(expected);
        Assert.NotNull(transform.PendingWrite);
        transform.PendingWrite!(expected, null);
        Assert.Equal(1, errors);
        Assert.Equal(1, closes);
        Assert.Equal(0, transform.writableLength);
        Assert.Throws<InvalidOperationException>(() => transform.PendingWrite!(null, null));
    }

    [Fact]
    public void Destruction_SettlesPendingFinalizationWithoutSuccessfulFinish()
    {
        var transform = new DeferredTransform();
        var finishes = 0;
        transform.on("finish", () => finishes++);
        transform.end();
        transform.destroy();
        Assert.NotNull(transform.PendingFlush);
        transform.PendingFlush!(null);
        Assert.Equal(0, finishes);
        Assert.False(transform.writableFinished);
        Assert.False(transform.push(Buffer.from("late output")));
        Assert.Equal(0, transform.readableLength);
        Assert.Throws<InvalidOperationException>(() => transform.PendingFlush!(null));
    }

    private sealed class SmallWritable : Writable
    {
        public SmallWritable() : base(1) { }
    }

    private sealed class DeferredTransform : Transform
    {
        public Action<Exception?, object?>? PendingWrite { get; private set; }
        public Action<Exception?>? PendingFlush { get; private set; }

        protected override void _transform(object? chunk, string? encoding, Action<Exception?, object?> callback)
        {
            PendingWrite = callback;
        }

        protected override void _flush(Action<Exception?> callback)
        {
            PendingFlush = callback;
        }
    }
}
