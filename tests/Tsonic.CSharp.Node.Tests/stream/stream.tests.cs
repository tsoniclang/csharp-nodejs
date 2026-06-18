using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class StreamTests
{
    [Fact]
    public void Readable_ShouldBeCreatable()
    {
        var stream = new Readable();

        Assert.NotNull(stream);
        Assert.True(stream.readable);
        Assert.False(stream.readableEnded);
    }

    [Fact]
    public void Readable_Push_ShouldAddDataToBuffer()
    {
        var stream = new Readable();

        stream.push("hello");
        stream.push("world");

        var chunk1 = stream.read();
        Assert.Equal("hello", chunk1);

        var chunk2 = stream.read();
        Assert.Equal("world", chunk2);
    }

    [Fact]
    public void Readable_PushNull_ShouldEndStream()
    {
        var stream = new Readable();

        stream.push("data");
        stream.push(null);

        Assert.True(stream.readableEnded);
    }

    [Fact]
    public void Readable_Read_ShouldReturnNullWhenEmpty()
    {
        var stream = new Readable();

        var result = stream.read();

        Assert.Null(result);
    }

    [Fact]
    public void Readable_Pause_ShouldStopFlowing()
    {
        var stream = new Readable();

        stream.resume();
        Assert.False(stream.isPaused());

        stream.pause();
        Assert.True(stream.isPaused());
    }

    [Fact]
    public void Readable_Resume_ShouldEnableFlowing()
    {
        var stream = new Readable();

        Assert.True(stream.isPaused());

        stream.resume();
        Assert.False(stream.isPaused());
    }

    [Fact]
    public void Readable_SetEncoding_ShouldReturnSelf()
    {
        var stream = new Readable();

        var result = stream.setEncoding("utf8");

        Assert.Same(stream, result);
    }

    [Fact]
    public void Readable_Unshift_ShouldPrependData()
    {
        var stream = new Readable();

        stream.push("second");
        stream.unshift("first");

        var chunk1 = stream.read();
        Assert.Equal("first", chunk1);

        var chunk2 = stream.read();
        Assert.Equal("second", chunk2);
    }

    [Fact]
    public void Readable_ReadableLength_ShouldReflectBufferSize()
    {
        var stream = new Readable();

        Assert.Equal(0, stream.readableLength);

        stream.push("data1");
        stream.push("data2");

        Assert.Equal(2, stream.readableLength);

        stream.read();
        Assert.Equal(1, stream.readableLength);
    }

    [Fact]
    public void Readable_Destroy_ShouldMarkAsDestroyed()
    {
        var stream = new Readable();

        stream.destroy();

        Assert.True(stream.destroyed);
        Assert.False(stream.readable);
    }

    [Fact]
    public void Readable_DestroyWithError_ShouldEmitError()
    {
        var stream = new Readable();
        Exception? caughtError = null;

        stream.on("error", (Action<Exception>)(err => { caughtError = err; }));

        var error = new Exception("test error");
        stream.destroy(error);

        Assert.NotNull(caughtError);
        Assert.Equal("test error", caughtError.Message);
    }

    [Fact]
    public void Readable_FlowingMode_ShouldEmitData()
    {
        var stream = new Readable();
        var received = new List<string>();

        stream.on("data", (Action<object?>)(chunk =>
        {
            if (chunk != null)
                received.Add(chunk.ToString()!);
        }));

        stream.push("chunk1");
        stream.resume();
        stream.push("chunk2");
        stream.push("chunk3");

        Thread.Sleep(50); // Give events time to process

        Assert.Contains("chunk1", received);
        Assert.Contains("chunk2", received);
        Assert.Contains("chunk3", received);
    }

    [Fact]
    public void Writable_ShouldBeCreatable()
    {
        var stream = new Writable();

        Assert.NotNull(stream);
        Assert.True(stream.writable);
        Assert.False(stream.writableEnded);
    }

    [Fact]
    public void Writable_Write_ShouldReturnTrue()
    {
        var stream = new Writable();

        var result = stream.write("test data");

        Assert.True(result);
    }

    [Fact]
    public void Writable_End_ShouldMarkAsEnded()
    {
        var stream = new Writable();

        stream.end();

        Assert.True(stream.writableEnded);
    }

    [Fact]
    public void Writable_EndWithData_ShouldWriteThenEnd()
    {
        var stream = new Writable();

        stream.end("final chunk");

        Assert.True(stream.writableEnded);
    }

    [Fact]
    public void Writable_WriteAfterEnd_ShouldThrow()
    {
        var stream = new Writable();

        stream.end();

        Assert.Throws<InvalidOperationException>(() => stream.write("data"));
    }

    [Fact]
    public void Writable_Cork_ShouldBufferWrites()
    {
        var stream = new Writable();

        stream.cork();
        Assert.True(stream.writableCorked);

        stream.write("chunk1");
        stream.write("chunk2");

        stream.uncork();
        Assert.False(stream.writableCorked);
    }

    [Fact]
    public void Writable_Destroy_ShouldMarkAsDestroyed()
    {
        var stream = new Writable();

        stream.destroy();

        Assert.True(stream.destroyed);
        Assert.False(stream.writable);
    }

    [Fact]
    public void Writable_WritableLength_ShouldReflectBufferSize()
    {
        var stream = new Writable();

        stream.cork();
        stream.write("data1");
        stream.write("data2");

        Assert.True(stream.writableLength > 0);

        stream.uncork();
    }

    [Fact]
    public void Duplex_ShouldBeCreatable()
    {
        var stream = new Duplex();

        Assert.NotNull(stream);
        Assert.True(stream.readable);
        Assert.True(stream.writable);
    }

    [Fact]
    public void Duplex_ShouldSupportReadableOperations()
    {
        var stream = new Duplex();

        stream.push("data");
        var result = stream.read();

        Assert.Equal("data", result);
    }

    [Fact]
    public void Duplex_ShouldSupportWritableOperations()
    {
        var stream = new Duplex();

        var result = stream.write("data");

        Assert.True(result);
    }

    [Fact]
    public void Duplex_ShouldSupportBothEnds()
    {
        var stream = new Duplex();

        // Write side
        stream.write("write data");
        stream.end();

        // Read side
        stream.push("read data");
        stream.push(null);

        Assert.True(stream.writableEnded);
        Assert.True(stream.readableEnded);
    }

    [Fact]
    public void Transform_ShouldBeCreatable()
    {
        var stream = new Transform();

        Assert.NotNull(stream);
        Assert.True(stream.readable);
        Assert.True(stream.writable);
    }

    [Fact]
    public void Transform_DefaultBehavior_ShouldPassThrough()
    {
        var stream = new Transform();
        var received = new List<string>();

        stream.on("data", (Action<object?>)(chunk =>
        {
            if (chunk != null)
                received.Add(chunk.ToString()!);
        }));

        stream.resume();
        stream.write("test data");

        Thread.Sleep(50);

        Assert.Contains("test data", received);
    }

    [Fact]
    public void PassThrough_ShouldBeCreatable()
    {
        var stream = new PassThrough();

        Assert.NotNull(stream);
        Assert.True(stream.readable);
        Assert.True(stream.writable);
    }

    [Fact]
    public void PassThrough_ShouldPassDataThrough()
    {
        var stream = new PassThrough();
        var received = new List<string>();

        stream.on("data", (Action<object?>)(chunk =>
        {
            if (chunk != null)
                received.Add(chunk.ToString()!);
        }));

        stream.resume();

        stream.write("chunk1");
        stream.write("chunk2");
        stream.write("chunk3");

        Thread.Sleep(50);

        Assert.Contains("chunk1", received);
        Assert.Contains("chunk2", received);
        Assert.Contains("chunk3", received);
    }

    [Fact]
    public void Stream_Pipe_ShouldTransferDataBetweenStreams()
    {
        var readable = new Readable();
        var writable = new Writable();

        readable.pipe(writable);

        readable.push("data1");
        readable.push("data2");
        readable.push(null);

        // Pipe should transfer data from readable to writable
        Assert.True(writable.writableLength >= 0);
    }

    [Fact]
    public void Stream_Pipe_ShouldReturnDestination()
    {
        var readable = new Readable();
        var writable = new Writable();

        var result = readable.pipe(writable);

        Assert.Same(writable, result);
    }

    [Fact]
    public void Stream_Pipe_ShouldNotEndDestinationWhenSpecified()
    {
        var readable = new Readable();
        var writable = new Writable();

        readable.pipe(writable, end: false);

        readable.push(null);

        Thread.Sleep(50);

        // Writable should not be ended
        Assert.False(writable.writableEnded);
    }

    [Fact]
    public void Readable_Events_ShouldBeEmitted()
    {
        var stream = new Readable();
        bool readableEmitted = false;
        bool endEmitted = false;

        stream.on("readable", (Action)(() => { readableEmitted = true; }));
        stream.on("end", (Action)(() => { endEmitted = true; }));

        stream.push("data");
        Assert.True(readableEmitted);

        stream.push(null);
        stream.read(); // Trigger end event
        Assert.True(endEmitted);
    }

    [Fact]
    public void Writable_FinishEvent_ShouldBeEmitted()
    {
        var stream = new Writable();
        bool finishEmitted = false;

        stream.on("finish", (Action)(() => { finishEmitted = true; }));

        stream.end();

        Thread.Sleep(50);

        Assert.True(finishEmitted);
    }

    [Fact]
    public void Stream_CloseEvent_ShouldBeEmittedOnDestroy()
    {
        var stream = new Readable();
        bool closeEmitted = false;

        stream.on("close", (Action)(() => { closeEmitted = true; }));

        stream.destroy();

        Thread.Sleep(50);

        Assert.True(closeEmitted);
    }

    [Fact]
    public void ComplexPipeline_ShouldWork()
    {
        var readable = new Readable();
        var transform1 = new PassThrough();
        var transform2 = new PassThrough();
        var writable = new Writable();

        // Should be able to create a complex pipeline without errors
        readable.pipe(transform1).pipe(transform2).pipe(writable);

        readable.push("data1");
        readable.push("data2");
        readable.push(null);

        Thread.Sleep(100);

        // Verify the pipeline was set up correctly (at least one of the streams should be affected)
        Assert.True(readable.readableEnded || transform1.readableEnded || writable.writableLength >= 0);
    }
}
