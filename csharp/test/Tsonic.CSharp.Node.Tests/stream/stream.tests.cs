using System;
using System.Collections.Generic;
using System.Threading;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public partial class StreamTests
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

        var ends = 0;
        JsEventLoop.Run(() =>
        {
            stream.on("end", () => ends++);
            stream.push("data");
            stream.push(null);
            Assert.False(stream.readableEnded);
            Assert.Equal("data", stream.read());
            Assert.False(stream.readableEnded);
        });
        Assert.True(stream.readableEnded);
        Assert.Equal(1, ends);
    }

    [Fact]
    public void Readable_Read_ShouldReturnNullWhenEmpty()
    {
        var stream = new Readable();

        var result = stream.read();

        Assert.Null(result);
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

        Assert.Equal(10, stream.readableLength);

        stream.read();
        Assert.Equal(5, stream.readableLength);
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

        var ends = 0;
        JsEventLoop.Run(() =>
        {
            stream.on("end", () => ends++);
            stream.write("write data");
            stream.end();
            stream.push("read data");
            stream.push(null);
            Assert.True(stream.writableEnded);
            Assert.False(stream.readableEnded);
            Assert.Equal("read data", stream.read());
            Assert.False(stream.readableEnded);
        });
        Assert.True(stream.readableEnded);
        Assert.Equal(1, ends);
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
    public void PassThrough_ShouldBeCreatable()
    {
        var stream = new PassThrough();

        Assert.NotNull(stream);
        Assert.True(stream.readable);
        Assert.True(stream.writable);
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

}
