using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public partial class StreamTests
{
    [Fact]
    public void Readable_PipeTo_ShouldPreserveTransformAndWritableIdentity()
    {
        var readable = new Readable();
        var transform = new PassThrough();
        var writable = new Writable();
        PassThrough selectedTransform = readable.pipeTo(transform);
        Writable selectedWritable = transform.pipeTo(writable);

        Assert.Same(transform, selectedTransform);
        Assert.Same(writable, selectedWritable);
        readable.push("native stream");
        readable.push(null);
        Assert.True(transform.writableEnded);
        Assert.True(writable.writableEnded);
    }

    [Fact]
    public void Stream_Pipe_ShouldRejectNonWritableDestinations()
    {
        var readable = new Readable();
        Assert.Throws<InvalidOperationException>(() => readable.pipe(new Readable()));
    }

    [Fact]
    public void Readable_PipeTo_ShouldWriteBufferedFinalChunkBeforeEndingDestination()
    {
        var source = Readable.from(new[] { Buffer.from("native stream") });
        var destination = new PassThrough();

        Assert.Same(destination, source.pipeTo(destination));
        Assert.True(destination.writableEnded);
        Assert.Equal("native stream", Assert.IsType<Buffer>(destination.read()).toString("utf8"));
    }

    [Fact]
    public void Writable_NativeProtocol_ShouldNotAddPerWriteAllocation()
    {
        var direct = new Writable();
        IWritableStream protocol = new Writable();
        for (var index = 0; index < 1000; index++)
        {
            direct.write("native");
            protocol.WriteChunk("native");
        }

        var start = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 10000; index++)
            direct.write("native");
        var directBytes = GC.GetAllocatedBytesForCurrentThread() - start;

        start = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 10000; index++)
            protocol.WriteChunk("native");
        var protocolBytes = GC.GetAllocatedBytesForCurrentThread() - start;

        Assert.Equal(directBytes, protocolBytes);
    }
}
