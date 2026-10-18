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
        ZlibTransform? codec = null;
        var closes = 0;
        var finishes = 0;
        var errors = 0;
        JsEventLoop.Run(() =>
        {
            codec = new ZlibTransform(mode);
            Readable projection = codec;
            projection.on("close", () => closes++);
            codec.on("finish", () => finishes++);
            codec.on<Exception>("error", _ => errors++);
            Assert.Same(codec, projection.destroyChain());
            projection.destroyChain();
        });
        Assert.False(ProcessKeepAlive.HasReferences);
        Assert.NotNull(codec);
        Assert.True(codec.destroyed);
        Assert.False(codec.readable);
        Assert.False(codec.writable);
        Assert.False(codec.writableFinished);
        Assert.Equal(1, closes);
        Assert.Equal(0, finishes);
        Assert.Equal(0, errors);
    }

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
    public void NormalCodecCompletion_QueuesFinalAcknowledgementBeforeReleasingTheProcessor(ZlibMode mode)
    {
        ZlibTransform? codec = null;
        var plain = Buffer.from("native completed codec");
        var input = mode switch
        {
            ZlibMode.Gunzip or ZlibMode.Unzip => zlib.gzipSync(plain),
            ZlibMode.Inflate => zlib.deflateSync(plain),
            ZlibMode.InflateRaw => zlib.deflateRawSync(plain),
            ZlibMode.BrotliDecompress => zlib.brotliCompressSync(plain),
            _ => plain,
        };
        var chunks = new List<Buffer>();
        var errors = new List<Exception>();
        var finishes = 0;
        JsEventLoop.Run(() =>
        {
            codec = new ZlibTransform(mode);
            codec.on<Buffer>("data", chunks.Add);
            codec.on<Exception>("error", errors.Add);
            codec.once("finish", () => finishes++);
            codec.write(Buffer.alloc(0));
            codec.write(input);
            codec.write(Buffer.alloc(0));
            codec.end();
        });
        Assert.False(ProcessKeepAlive.HasReferences);
        Assert.NotNull(codec);
        var output = Buffer.concat(chunks);
        var decoded = mode switch
        {
            ZlibMode.Gzip => zlib.gunzipSync(output),
            ZlibMode.Deflate => zlib.inflateSync(output),
            ZlibMode.DeflateRaw => zlib.inflateRawSync(output),
            ZlibMode.BrotliCompress => zlib.brotliDecompressSync(output),
            _ => output,
        };
        Assert.Equal(plain.toString(), decoded.toString());
        Assert.Empty(errors);
        Assert.Equal(1, finishes);
        Assert.True(codec.writableFinished);
        Assert.True(codec.readableEnded);
        Assert.Equal(0, codec.writableLength);
    }

    [Fact]
    public void ConcurrentIdleCodecs_DoNotBlockIndependentCodecProgress()
    {
        var idle = new ZlibTransform[12];
        ZlibTransform? active = null;
        var finishes = 0;
        var chunks = new List<Buffer>();
        try
        {
            JsEventLoop.Run(() =>
            {
                for (var index = 0; index < idle.Length; index++)
                    idle[index] = zlib.createGzip();
                active = zlib.createGzip();
                active.on<Buffer>("data", chunks.Add);
                active.once("finish", () =>
                {
                    finishes++;
                    foreach (var codec in idle)
                        codec.destroy();
                });
                active.end(Buffer.from("independent native progress"));
            });
            Assert.False(ProcessKeepAlive.HasReferences);
        }
        finally
        {
            foreach (var codec in idle)
                codec?.destroy();
            active?.destroy();
            JsEventLoop.Run();
            Assert.False(ProcessKeepAlive.HasReferences);
        }
        Assert.Equal(1, finishes);
        Assert.Equal("independent native progress", zlib.gunzipSync(Buffer.concat(chunks)).toString());
        Assert.All(idle, codec => Assert.True(codec.destroyed));
    }

    [Fact]
    public void CodecInputFailure_ClosesItsProcessorWithoutSuccessfulFinish()
    {
        ZlibTransform? codec = null;
        var errors = new List<Exception>();
        var closes = 0;
        var finishes = 0;
        JsEventLoop.Run(() =>
        {
            codec = zlib.createGunzip();
            codec.once<Exception>("error", errors.Add);
            codec.once("close", () => closes++);
            codec.on("finish", () => finishes++);
            codec.end(Buffer.from("not compressed data"));
        });
        Assert.False(ProcessKeepAlive.HasReferences);
        Assert.NotNull(codec);
        Assert.Single(errors);
        Assert.Equal(1, closes);
        Assert.Equal(0, finishes);
        Assert.True(codec.destroyed);
        Assert.False(codec.writableFinished);
        Assert.Equal(0, codec.writableLength);
        Assert.Throws<InvalidOperationException>(() => codec.write(Buffer.from("late input")));
    }

    [Fact]
    public void PendingCodecInputAndPublication_AbortRetainsOneErrorAndNoFinish()
    {
        ZlibTransform? codec = null;
        var expected = new InvalidOperationException("abort pending native codec work");
        var errors = new List<Exception>();
        var closes = 0;
        var finishes = 0;
        JsEventLoop.Run(() =>
        {
            codec = zlib.createGzip();
            Duplex projection = codec;
            codec.on<Exception>("error", errors.Add);
            codec.on("close", () => closes++);
            codec.on("finish", () => finishes++);
            codec.write(Buffer.alloc(256 * 1024));
            codec.end();
            Assert.Same(codec, projection.destroyChain(expected));
            projection.destroyChain(expected);
        });
        Assert.False(ProcessKeepAlive.HasReferences);
        Assert.NotNull(codec);
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
        ZlibTransform? codec = null;
        var expected = new InvalidOperationException("abort native output pressure");
        var errors = new List<Exception>();
        var pressureObserved = false;
        var closes = 0;
        var finishes = 0;
        JsEventLoop.Run(() =>
        {
            codec = zlib.createGunzip();
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
        });
        Assert.False(ProcessKeepAlive.HasReferences);
        Assert.NotNull(codec);
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
        ZlibTransform? codec = null;
        var expected = new InvalidOperationException("native output listener failure");
        var errors = new List<Exception>();
        var closes = 0;
        JsEventLoop.Run(() =>
        {
            codec = zlib.createGzip();
            codec.on<Buffer>("data", _ => throw expected);
            codec.on<Exception>("error", errors.Add);
            codec.on("close", () => closes++);
            codec.end(Buffer.from("native callback identity"));
        });
        Assert.False(ProcessKeepAlive.HasReferences);
        Assert.NotNull(codec);
        Assert.Collection(errors, error => Assert.Same(expected, error));
        Assert.Equal(1, closes);
        Assert.True(codec.destroyed);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DestructionInsideOutputCallback_DoesNotHideAThrowingErrorListener(bool throwsCancellation)
    {
        ZlibTransform? codec = null;
        var supplied = new Exception("native supplied error");
        Exception expected = throwsCancellation
            ? new OperationCanceledException("native error listener failure")
            : new InvalidOperationException("native error listener failure");
        var closes = 0;
        try
        {
            Assert.Same(expected, Assert.ThrowsAny<Exception>(() => JsEventLoop.Run(() =>
            {
                codec = zlib.createGzip();
                codec.on("close", () => closes++);
                codec.once<Exception>("error", error =>
                {
                    Assert.Same(supplied, error);
                    throw expected;
                });
                codec.on<Buffer>("data", _ => codec.destroy(supplied));
                codec.end(Buffer.from("native nested destruction"));
            })));
        }
        finally
        {
            codec?.destroy();
            JsEventLoop.Run();
            Assert.False(ProcessKeepAlive.HasReferences);
        }
        Assert.NotNull(codec);
        Assert.Equal(1, closes);
        Assert.True(codec.destroyed);
    }

}
