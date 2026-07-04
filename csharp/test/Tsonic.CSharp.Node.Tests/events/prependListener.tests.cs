using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class prependListenerTests
{
    [Fact]
    public void prependListener_ShouldAddToBeginning()
    {
        var emitter = new EventEmitter();
        var order = new List<int>();

        emitter.on("test", () => order.Add(1));
        emitter.on("test", () => order.Add(2));
        emitter.prependListener("test", () => order.Add(3));

        emitter.emit("test");

        Assert.Equal(new[] { 3, 1, 2 }, order);
    }

    [Fact]
    public void prependOnceListener_ShouldAddToBeginningAndCallOnce()
    {
        var emitter = new EventEmitter();
        var order = new List<int>();

        emitter.on("test", () => order.Add(1));
        emitter.prependOnceListener("test", () => order.Add(2));

        emitter.emit("test");
        emitter.emit("test");

        Assert.Equal(new[] { 2, 1, 1 }, order);
    }

    [Fact]
    public void addListener_ShouldBeAliasForOn()
    {
        var emitter = new EventEmitter();
        var count = 0;

        emitter.addListener("test", () => count++);
        emitter.emit("test");

        Assert.Equal(1, count);
    }
}
