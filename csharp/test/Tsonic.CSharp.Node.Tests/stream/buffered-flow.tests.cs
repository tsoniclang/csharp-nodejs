using System;
using System.Collections.Generic;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class BufferedReadableFlowTests
{
    [Fact]
    public void BufferedDataWaitsForRegistrationAndPrecedesEnd()
    {
        var chunk = Buffer.from("answer\n");
        var input = Readable.from(new[] { chunk });
        var events = new List<string>();
        input.on("data", (Action<Buffer>)(value =>
        {
            Assert.Same(chunk, value);
            events.Add("data");
        }));
        input.on("end", (Action)(() => events.Add("end")));
        Assert.Empty(events);
        JsEventLoop.Run();
        Assert.Equal(new[] { "data", "end" }, events);
        JsEventLoop.Run();
        Assert.Equal(2, events.Count);
    }

    [Fact]
    public void PausingBeforeDispatchRetainsTheSameBufferedChunk()
    {
        var chunk = Buffer.from("paused");
        var input = Readable.from(new[] { chunk });
        var delivered = 0;
        input.on("data", (Action<Buffer>)(_ => delivered++));
        input.pause();
        JsEventLoop.Run();
        Assert.Equal(0, delivered);
        Assert.Same(chunk, input.read());
    }

    [Fact]
    public void ReadlineQuestionReceivesBufferedInputBeforeClose()
    {
        var reader = readline.createInterface(Readable.from(new[] { Buffer.from("answer\n") }));
        var events = new List<string>();
        reader.question("", answer => events.Add(answer));
        reader.on("close", (Action)(() => events.Add("close")));
        Assert.Empty(events);
        JsEventLoop.Run();
        Assert.Equal(new[] { "answer", "close" }, events);
    }

    [Fact]
    public void ReadlineCallbackFailureRetainsExceptionIdentity()
    {
        var reader = readline.createInterface(Readable.from(new[] { Buffer.from("answer\n") }));
        var expected = new InvalidOperationException("readline callback");
        reader.question("", _ => throw expected);
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(JsEventLoop.Run));
        reader.close();
        JsEventLoop.Run();
    }
}
