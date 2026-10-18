using JsEventLoop = Tsonic.CSharp.Js.JsEventLoop;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using Tsonic.CSharp.Node.Http;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

[Collection(JsEventLoopCollection.Name)]
public class NativeBodyBufferCostTests
{
    [Fact]
    public async Task MaterializedBodyPreservesChunkOrderEmptyChunksAndIndependentStorage()
    {
        var connections = Enumerable.Range(0, 2).Select(index => new DefaultConnectionContext()).ToArray();
        var closedSignals = connections.Select(connection =>
        {
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var registration = connection.ConnectionClosed.Register(static state => ((TaskCompletionSource)state!).TrySetResult(), completion);
            return (Completion: completion, Registration: registration);
        }).ToArray();
        var admitted = new List<DefaultConnectionContext>();
        try
        {
            JsEventLoop.Run(() =>
            {
                var first = Buffer.from("0080ff", "hex");
                var second = Buffer.from("ab", "hex");
                var (body, _) = Materialize(connections[0], new[] { first, Buffer.alloc(0), second }, admitted);
                Assert.Equal("0080ffab", body.toString("hex"));
                first[1] = 0;
                second[0] = 0;
                Assert.Equal("0080ffab", body.toString("hex"));
                Assert.Equal(0, Materialize(connections[1], Array.Empty<Buffer>(), admitted).Body.length);
            });
            Assert.Equal(connections.Length, admitted.Count);
        }
        finally
        {
            await Task.WhenAll(closedSignals.Where((signal, index) => admitted.Contains(connections[index])).Select(signal => signal.Completion.Task));
            foreach (var connection in admitted) Assert.True(connection.ConnectionClosed.IsCancellationRequested);
            foreach (var signal in closedSignals) signal.Registration.Dispose();
            foreach (var connection in connections) await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task BodyCompletionDoesNotAllocateAnArrayProportionalToChunkCount()
    {
        var connections = Enumerable.Range(0, 34).Select(index => new DefaultConnectionContext()).ToArray();
        var closedSignals = connections.Select(connection =>
        {
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var registration = connection.ConnectionClosed.Register(static state => ((TaskCompletionSource)state!).TrySetResult(), completion);
            return (Completion: completion, Registration: registration);
        }).ToArray();
        var admitted = new List<DefaultConnectionContext>();
        try
        {
            JsEventLoop.Run(() =>
            {
                var empty = Buffer.alloc(0);
                var single = new[] { empty };
                var many = new Buffer[512];
                Array.Fill(many, empty);
                for (var index = 0; index < 16; index++)
                {
                    _ = Materialize(connections[index * 2], single, admitted);
                    _ = Materialize(connections[index * 2 + 1], many, admitted);
                }
                var (first, singleAllocated) = Materialize(connections[32], single, admitted);
                var (second, manyAllocated) = Materialize(connections[33], many, admitted);
                Assert.Equal(0, first.length);
                Assert.Equal(0, second.length);
                Assert.Equal(singleAllocated, manyAllocated);
            });
            Assert.Equal(connections.Length, admitted.Count);
        }
        finally
        {
            await Task.WhenAll(closedSignals.Where((signal, index) => admitted.Contains(connections[index])).Select(signal => signal.Completion.Task));
            foreach (var connection in admitted) Assert.True(connection.ConnectionClosed.IsCancellationRequested);
            foreach (var signal in closedSignals) signal.Registration.Dispose();
            foreach (var connection in connections) await connection.DisposeAsync();
        }
    }

    private static (Buffer Body, long Allocated) Materialize(DefaultConnectionContext connection, IReadOnlyList<Buffer> chunks, List<DefaultConnectionContext> admitted)
    {
        var context = new DefaultHttpContext();
        context.Features.Set<IConnectionLifetimeFeature>(connection.Features.GetRequiredFeature<IConnectionLifetimeFeature>());
        var message = new IncomingMessage(context.Request, Tsonic.CSharp.Js.JsEventLoop.Capture());
        admitted.Add(connection);
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
