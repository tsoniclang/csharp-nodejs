using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class offTests
{
    [Fact]
    public void off_ShouldBeAliasForRemoveListener()
    {
        var emitter = new EventEmitter();
        var called = false;
        Action listener = () => called = true;

        emitter.on("test", listener);
        emitter.off("test", listener);
        emitter.emit("test");

        Assert.False(called);
    }

    [Fact]
    public void off_ShouldReturnEmitterForChaining()
    {
        var emitter = new EventEmitter();
        Action listener = () => { };
        emitter.on("test", listener);

        var result = emitter.off("test", listener);

        Assert.Same(emitter, result);
    }

    [Fact]
    public void off_ShouldRemoveSpecificListener()
    {
        var emitter = new EventEmitter();
        var count = 0;
        Action listener1 = () => count++;
        Action listener2 = () => count++;

        emitter.on("test", listener1);
        emitter.on("test", listener2);
        emitter.off("test", listener1);
        emitter.emit("test");

        Assert.Equal(1, count);
    }

    [Fact]
    public void off_NonExistentListener_ShouldNotThrow()
    {
        var emitter = new EventEmitter();
        Action listener = () => { };

        emitter.off("test", listener);
    }
}
