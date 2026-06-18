using System;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class EventEmitterOnceStaticTests
{
    [Fact]
    public async Task Once_Static_ResolvesWhenEventEmitted()
    {
        var emitter = new EventEmitter();

        // Start waiting for event
        var task = EventEmitter.once(emitter, "test");

        // Emit the event
        emitter.emit("test", "arg1", "arg2");

        // Should resolve with the arguments
        var args = await task;
        Assert.Equal(2, args.Length);
        Assert.Equal("arg1", args[0]);
        Assert.Equal("arg2", args[1]);
    }

    [Fact]
    public async Task Once_Static_WorksWithNoArgs()
    {
        var emitter = new EventEmitter();

        var task = EventEmitter.once(emitter, "noargs");
        emitter.emit("noargs");

        var args = await task;
        Assert.Empty(args);
    }

    [Fact]
    public async Task Once_Static_OnlyListensOnce()
    {
        var emitter = new EventEmitter();

        var task = EventEmitter.once(emitter, "once");

        // Emit twice
        emitter.emit("once", 1);
        emitter.emit("once", 2);

        // Should only get first emission
        var args = await task;
        Assert.Single(args);
        Assert.Equal(1, args[0]);
    }

    [Fact]
    public void Once_Static_NullEmitter_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            var _ = EventEmitter.once(null!, "test");
        });
    }

    [Fact]
    public void Once_Static_NullEventName_Throws()
    {
        var emitter = new EventEmitter();
        Assert.Throws<ArgumentException>(() =>
        {
            var _ = EventEmitter.once(emitter, null!);
        });
    }
}
