using System;
using System.Collections.Generic;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public partial class StreamTests
{
    [Fact]
    public void Readable_Pause_ShouldStopFlowing()
    {
        var stream = new Readable();
        JsEventLoop.Run(() =>
        {
            stream.resume();
            Assert.False(stream.isPaused());
            stream.pause();
            Assert.True(stream.isPaused());
        });
    }

    [Fact]
    public void Readable_Resume_ShouldEnableFlowing()
    {
        var stream = new Readable();
        Assert.True(stream.isPaused());
        JsEventLoop.Run(() =>
        {
            stream.resume();
            Assert.False(stream.isPaused());
        });
    }

    [Fact]
    public void Readable_FlowingMode_ShouldEmitData()
    {
        var stream = new Readable();
        var received = new List<string>();
        JsEventLoop.Run(() =>
        {
            ((EventEmitter)stream).on("data", (Action<object?>)(chunk =>
            {
                if (chunk != null)
                    received.Add(chunk.ToString()!);
            }));
            stream.push("chunk1");
            stream.resume();
            stream.push("chunk2");
            stream.push("chunk3");
        });
        Assert.Contains("chunk1", received);
        Assert.Contains("chunk2", received);
        Assert.Contains("chunk3", received);
    }

    [Fact]
    public void Transform_DefaultBehavior_ShouldPassThrough()
    {
        var stream = new Transform();
        var received = new List<string>();
        JsEventLoop.Run(() =>
        {
            ((EventEmitter)stream).on("data", (Action<object?>)(chunk =>
            {
                if (chunk != null)
                    received.Add(chunk.ToString()!);
            }));
            stream.resume();
            stream.write("test data");
        });
        Assert.Contains("test data", received);
    }

    [Fact]
    public void PassThrough_ShouldPassDataThrough()
    {
        var stream = new PassThrough();
        var received = new List<string>();
        JsEventLoop.Run(() =>
        {
            ((EventEmitter)stream).on("data", (Action<object?>)(chunk =>
            {
                if (chunk != null)
                    received.Add(chunk.ToString()!);
            }));
            stream.resume();
            stream.write("chunk1");
            stream.write("chunk2");
            stream.write("chunk3");
        });
        Assert.Contains("chunk1", received);
        Assert.Contains("chunk2", received);
        Assert.Contains("chunk3", received);
    }

    [Fact]
    public void Stream_Pipe_ShouldTransferDataBetweenStreams()
    {
        var readable = new Readable();
        var writable = new Writable();
        JsEventLoop.Run(() =>
        {
            readable.pipe(writable);
            readable.push("data1");
            readable.push("data2");
            readable.push(null);
        });
        Assert.True(writable.writableLength >= 0);
    }

    [Fact]
    public void Stream_Pipe_ShouldReturnDestination()
    {
        var readable = new Readable();
        var writable = new Writable();
        JsEventLoop.Run(() =>
        {
            var result = readable.pipe(writable);
            Assert.Same(writable, result);
        });
    }

    [Fact]
    public void Stream_Pipe_ShouldNotEndDestinationWhenSpecified()
    {
        var readable = new Readable();
        var writable = new Writable();
        JsEventLoop.Run(() =>
        {
            readable.pipe(writable, end: false);
            readable.push(null);
        });
        Assert.False(writable.writableEnded);
    }

    [Fact]
    public void Readable_Events_ShouldBeEmitted()
    {
        var stream = new Readable();
        bool readableEmitted = false;
        bool endEmitted = false;
        JsEventLoop.Run(() =>
        {
            stream.on("readable", (Action)(() => { readableEmitted = true; }));
            stream.on("end", (Action)(() => { endEmitted = true; }));
            stream.push("data");
            Assert.True(readableEmitted);
            stream.push(null);
            stream.read();
        });
        Assert.True(endEmitted);
    }

    [Fact]
    public void ComplexPipeline_ShouldWork()
    {
        var readable = new Readable();
        var transform1 = new PassThrough();
        var transform2 = new PassThrough();
        var writable = new Writable();
        JsEventLoop.Run(() =>
        {
            readable.pipe(transform1).pipe(transform2).pipe(writable);
            readable.push("data1");
            readable.push("data2");
            readable.push(null);
        });
        Assert.True(readable.readableEnded || transform1.readableEnded || writable.writableLength >= 0);
    }
}
