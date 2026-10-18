using System;
using System.Collections.Generic;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class BufferedReadableFlowTests
{
    [Fact]
    public void SynchronousInputUsesOneNonrecursiveConsumptionPump()
    {
        var input = new SynchronousInput();
        var received = 0;
        var ends = 0;
        JsEventLoop.Run(() =>
        {
            input.on<Buffer>("data", chunk =>
            {
                Assert.Same(input.Chunk, chunk);
                Assert.Equal(0, ends);
                received++;
                input.pause();
                input.resume();
            });
            input.on("end", () => ends++);
        });
        Assert.Equal(10_000, received);
        Assert.Equal(1, input.MaximumDepth);
        Assert.Equal(1, ends);
        Assert.False(ProcessKeepAlive.HasReferences);
    }

    [Fact]
    public void FinalExplicitConsumptionEmitsEndOnceAndAfterAllBytes()
    {
        var input = Readable.from(new[] { Buffer.from("data") });
        var endings = 0;
        JsEventLoop.Run(() =>
        {
            input.on("end", (Action)(() => {
                endings++;
                Assert.Equal(0, input.readableLength);
                Assert.Null(input.read());
            }));
            Assert.Equal("da", input.readBuffer(2)!.toString());
            Assert.Equal(0, endings);
            Assert.Equal("ta", input.readBuffer(2)!.toString());
            Assert.Equal(0, endings);
            Assert.False(input.readableEnded);
        });
        Assert.Equal(1, endings);
        Assert.True(input.readableEnded);
        Assert.Null(input.read());
        Assert.Equal(1, endings);
    }

    [Fact]
    public void EmptyFinalChunkStillHasOneConsumptionAndOneEnd()
    {
        var chunk = Buffer.from("");
        var input = Readable.from(new[] { chunk });
        var endings = 0;
        JsEventLoop.Run(() =>
        {
            input.on("end", (Action)(() => endings++));
            Assert.Same(chunk, input.read());
            Assert.Equal(0, endings);
        });
        Assert.Equal(1, endings);
        Assert.Null(input.read());
        Assert.Equal(1, endings);
    }

    [Fact]
    public void BufferedDataWaitsForRegistrationAndPrecedesEnd()
    {
        var chunk = Buffer.from("answer\n");
        var input = Readable.from(new[] { chunk });
        var events = new List<string>();
        JsEventLoop.Run(() =>
        {
            input.on("data", (Action<Buffer>)(value =>
            {
                Assert.Same(chunk, value);
                events.Add("data");
            }));
            input.on("end", (Action)(() => events.Add("end")));
            Assert.Empty(events);
        });
        Assert.Equal(new[] { "data", "end" }, events);
        JsEventLoop.Run();
        Assert.Equal(2, events.Count);
    }

    [Fact]
    public void EmptyStreamsWaitForConsumptionAndLateOnceDoesNotReplay()
    {
        var input = Readable.from(System.Array.Empty<Buffer>());
        var endings = 0;
        JsEventLoop.Run(() =>
        {
            input.on("end", (Action)(() => endings++));
            Assert.Equal(0, endings);
            Assert.Null(input.read());
            Assert.Equal(0, endings);
        });
        Assert.Equal(1, endings);
        input.once("end", (Action)(() => endings++));
        Assert.Equal(1, endings);
        JsEventLoop.Run();
        Assert.Equal(1, endings);
    }

    [Fact]
    public void ResumeWithoutDataListenerDrainsAndDestructionCancelsQueuedEnd()
    {
        var input = Readable.from(new[] { Buffer.from("discard") });
        var endings = 0;
        JsEventLoop.Run(() =>
        {
            input.on("end", (Action)(() => endings++));
            input.resume();
            Assert.Equal(0, endings);
        });
        Assert.Equal(0, input.readableLength);
        Assert.Equal(1, endings);

        var destroyed = Readable.from(System.Array.Empty<Buffer>());
        JsEventLoop.Run(() =>
        {
            destroyed.on("end", (Action)(() => endings++));
            Assert.Null(destroyed.read());
            destroyed.destroy();
        });
        Assert.Equal(1, endings);
        Assert.Null(destroyed.read());
    }

    [Fact]
    public void PausingBeforeDispatchRetainsTheSameBufferedChunk()
    {
        var chunk = Buffer.from("paused");
        var input = Readable.from(new[] { chunk });
        var delivered = 0;
        JsEventLoop.Run(() =>
        {
            input.on("data", (Action<Buffer>)(_ => delivered++));
            input.pause();
        });
        Assert.Equal(0, delivered);
        Assert.Same(chunk, input.read());
    }

    [Fact]
    public void ReadlineQuestionReceivesBufferedInputBeforeClose()
    {
        var events = new List<string>();
        JsEventLoop.Run(() =>
        {
            var reader = readline.createInterface(Readable.from(new[] { Buffer.from("answer\n") }));
            reader.question("", answer => events.Add(answer));
            reader.on("close", (Action)(() => events.Add("close")));
            Assert.Empty(events);
        });
        Assert.Equal(new[] { "answer", "close" }, events);
    }

    [Fact]
    public void ReadlineCallbackFailureRetainsExceptionIdentity()
    {
        Interface? reader = null;
        var expected = new InvalidOperationException("readline callback");
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => JsEventLoop.Run(() =>
        {
            reader = readline.createInterface(Readable.from(new[] { Buffer.from("answer\n") }));
            reader.question("", _ => throw expected);
        })));
        Assert.NotNull(reader);
        reader.close();
        JsEventLoop.Run();
    }

    private sealed class SynchronousInput : Readable
    {
        private int _remaining = 10_000;
        private int _depth;
        public Buffer Chunk { get; } = Buffer.from("x");
        public int MaximumDepth { get; private set; }

        protected override void _read(int size)
        {
            _depth++;
            MaximumDepth = System.Math.Max(MaximumDepth, _depth);
            try
            {
                push(_remaining-- > 0 ? Chunk : null);
            }
            finally
            {
                _depth--;
            }
        }
    }
}
