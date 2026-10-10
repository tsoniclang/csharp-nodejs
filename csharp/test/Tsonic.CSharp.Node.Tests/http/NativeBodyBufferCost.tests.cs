using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Tsonic.CSharp.Node.Http;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class NativeBodyBufferCostTests
{
    [Fact]
    public void MaterializedBodyPreservesChunkOrderEmptyChunksAndIndependentStorage()
    {
        var first = Buffer.from("0080ff", "hex");
        var second = Buffer.from("ab", "hex");
        var (body, _) = Materialize(new[] { first, Buffer.alloc(0), second });
        Assert.Equal("0080ffab", body.toString("hex"));
        first[1] = 0;
        second[0] = 0;
        Assert.Equal("0080ffab", body.toString("hex"));
        Assert.Equal(0, Materialize(Array.Empty<Buffer>()).Body.length);
    }

    [Fact]
    public void BodyCompletionDoesNotAllocateAnArrayProportionalToChunkCount()
    {
        var empty = Buffer.alloc(0);
        var single = new[] { empty };
        var many = new Buffer[512];
        Array.Fill(many, empty);
        for (var index = 0; index < 16; index++)
        {
            _ = Materialize(single);
            _ = Materialize(many);
        }
        var (first, singleAllocated) = Materialize(single);
        var (second, manyAllocated) = Materialize(many);
        Assert.Equal(0, first.length);
        Assert.Equal(0, second.length);
        Assert.Equal(singleAllocated, manyAllocated);
    }

    private static (Buffer Body, long Allocated) Materialize(IReadOnlyList<Buffer> chunks)
    {
        var message = new IncomingMessage(new DefaultHttpContext().Request);
        var result = message.readAllBuffer();
        Assert.Throws<InvalidOperationException>(() => { _ = message.readAllBuffer(); });
        foreach (var chunk in chunks)
            message.emit("data", chunk);
        var before = GC.GetAllocatedBytesForCurrentThread();
        message.emit("end");
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.True(result.IsCompletedSuccessfully);
        Assert.Equal(0, message.listenerCount("data"));
        Assert.Equal(0, message.listenerCount("end"));
        Assert.Equal(0, message.listenerCount("error"));
        Assert.Equal(0, message.listenerCount("aborted"));
        message.destroy();
        return (result.Result, allocated);
    }
}
