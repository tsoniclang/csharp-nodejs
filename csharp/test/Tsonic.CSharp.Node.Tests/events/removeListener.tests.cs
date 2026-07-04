using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class removeListenerTests
{
    [Fact]
    public void removeListener_ShouldRemoveSpecificListener()
    {
        var emitter = new EventEmitter();
        var count = 0;
        Action listener = () => count++;

        emitter.on("test", listener);
        emitter.emit("test");
        Assert.Equal(1, count);

        emitter.removeListener("test", listener);
        emitter.emit("test");
        Assert.Equal(1, count); // Should still be 1, not 2
    }

    [Fact]
    public void off_ShouldBeAliasForRemoveListener()
    {
        var emitter = new EventEmitter();
        var count = 0;
        Action listener = () => count++;

        emitter.on("test", listener);
        emitter.emit("test");

        emitter.off("test", listener);
        emitter.emit("test");

        Assert.Equal(1, count);
    }

    [Fact]
    public void removeListenerEvent_ShouldBeEmitted()
    {
        var emitter = new EventEmitter();
        string? eventName = null;
        Action listener = () => { };

        emitter.on("removeListener", (Action<object?, object?>)((name, listenerObj) =>
        {
            eventName = name as string;
        }));

        emitter.on("test", listener);
        emitter.removeListener("test", listener);

        Assert.Equal("test", eventName);
    }
}
