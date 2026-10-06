using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class NativeCodecLifecycleTests
{
    [Theory]
    [InlineData(ZlibMode.Gzip)]
    [InlineData(ZlibMode.Gunzip)]
    [InlineData(ZlibMode.Deflate)]
    [InlineData(ZlibMode.Inflate)]
    [InlineData(ZlibMode.DeflateRaw)]
    [InlineData(ZlibMode.InflateRaw)]
    [InlineData(ZlibMode.BrotliCompress)]
    [InlineData(ZlibMode.BrotliDecompress)]
    [InlineData(ZlibMode.Unzip)]
    public void IdleCodec_DestructionThroughReadableReleasesItsProcessor(ZlibMode mode)
    {
        var codec = new ZlibTransform(mode);
        Readable projection = codec;
        var closes = 0;
        var finishes = 0;
        var errors = 0;
        projection.on("close", () => closes++);
        codec.on("finish", () => finishes++);
        codec.on<Exception>("error", _ => errors++);
        Assert.Same(codec, projection.destroyChain());
        projection.destroyChain();
        DrainEventLoop();
        Assert.True(codec.destroyed);
        Assert.False(codec.readable);
        Assert.False(codec.writable);
        Assert.False(codec.writableFinished);
        Assert.Equal(1, closes);
        Assert.Equal(0, finishes);
        Assert.Equal(0, errors);
    }

    [Fact]
    public void PendingCodecInputAndPublication_AbortRetainsOneErrorAndNoFinish()
    {
        var codec = zlib.createGzip();
        Duplex projection = codec;
        var expected = new InvalidOperationException("abort pending native codec work");
        var errors = new List<Exception>();
        var closes = 0;
        var finishes = 0;
        codec.on<Exception>("error", errors.Add);
        codec.on("close", () => closes++);
        codec.on("finish", () => finishes++);
        codec.write(Buffer.alloc(256 * 1024));
        codec.end();
        Assert.Same(codec, projection.destroyChain(expected));
        projection.destroyChain(expected);
        DrainEventLoop();
        Assert.Collection(errors, error => Assert.Same(expected, error));
        Assert.Equal(1, closes);
        Assert.Equal(0, finishes);
        Assert.False(codec.writableFinished);
        Assert.Equal(0, codec.readableLength);
        Assert.Equal(0, codec.writableLength);
    }

    [Fact]
    public void BackpressuredCodecOutput_DestructionReleasesTheCapacityWait()
    {
        var codec = zlib.createGunzip();
        var expected = new InvalidOperationException("abort native output pressure");
        var errors = new List<Exception>();
        var pressureObserved = false;
        var closes = 0;
        var finishes = 0;
        codec.on<Exception>("error", errors.Add);
        codec.on("close", () => closes++);
        codec.on("finish", () => finishes++);
        codec.on("readable", () =>
        {
            if (codec.readableLength >= 64 * 1024)
            {
                pressureObserved = true;
                codec.destroy(expected);
            }
        });
        codec.end(zlib.gzipSync(Buffer.alloc(256 * 1024)));
        DrainEventLoop();
        Assert.True(pressureObserved);
        Assert.Collection(errors, error => Assert.Same(expected, error));
        Assert.Equal(1, closes);
        Assert.Equal(0, finishes);
        Assert.False(codec.writableFinished);
        Assert.Equal(0, codec.readableLength);
    }

    [Fact]
    public void CodecOutputCallbackFailure_ClosesTheProcessorWithTheOriginalError()
    {
        var codec = zlib.createGzip();
        var expected = new InvalidOperationException("native output listener failure");
        var errors = new List<Exception>();
        var closes = 0;
        codec.on<Buffer>("data", _ => throw expected);
        codec.on<Exception>("error", errors.Add);
        codec.on("close", () => closes++);
        codec.end(Buffer.from("native callback identity"));
        DrainEventLoop();
        Assert.Collection(errors, error => Assert.Same(expected, error));
        Assert.Equal(1, closes);
        Assert.True(codec.destroyed);
    }

    private static void DrainEventLoop()
    {
        var loop = Task.Factory.StartNew(
            JsEventLoop.Run,
            CancellationToken.None,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        loop.WaitAsync(TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        Assert.False(ProcessKeepAlive.HasReferences);
    }
}
