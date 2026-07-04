using System;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class eventsModuleTests
{
    [Fact]
    public async Task once_ShouldResolveOnNextEvent()
    {
        var emitter = new EventEmitter();
        var task = events.once(emitter, "ready");
        emitter.emit("ready", 123);

        var args = await task;
        Assert.Single(args);
        Assert.Equal(123, args[0]);
    }

    [Fact]
    public void listenerHelpers_ShouldReflectEmitterState()
    {
        var emitter = new EventEmitter();
        Action listener = () => { };

        emitter.on("tick", listener);

        Assert.Equal(1, events.listenerCount(emitter, "tick"));
        Assert.Single(events.getEventListeners(emitter, "tick"));

        events.setMaxListeners(5, emitter);
        Assert.Equal(5, events.getMaxListeners(emitter));
    }
}
