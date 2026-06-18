using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class addListenerTests
{
    [Fact]
    public void addListener_ShouldBeAliasForOn()
    {
        var emitter = new EventEmitter();
        var called = false;

        emitter.addListener("test", () => called = true);
        emitter.emit("test");

        Assert.True(called);
    }

    [Fact]
    public void addListener_ShouldReturnEmitterForChaining()
    {
        var emitter = new EventEmitter();
        var result = emitter.addListener("test", () => { });

        Assert.Same(emitter, result);
    }

    [Fact]
    public void addListener_ShouldAddToEndOfListenersArray()
    {
        var emitter = new EventEmitter();
        var order = new List<int>();

        emitter.addListener("test", () => order.Add(1));
        emitter.addListener("test", () => order.Add(2));
        emitter.addListener("test", () => order.Add(3));

        emitter.emit("test");

        Assert.Equal(new[] { 1, 2, 3 }, order);
    }
}
