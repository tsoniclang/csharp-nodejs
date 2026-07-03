using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class prependOnceListenerTests
{
    [Fact]
    public void prependOnceListener_ShouldExecuteOnlyOnce()
    {
        var emitter = new EventEmitter();
        var count = 0;

        emitter.prependOnceListener("test", () => count++);
        emitter.emit("test");
        emitter.emit("test");

        Assert.Equal(1, count);
    }

    [Fact]
    public void prependOnceListener_ShouldAddToBeginning()
    {
        var emitter = new EventEmitter();
        var order = new List<int>();

        emitter.on("test", () => order.Add(1));
        emitter.on("test", () => order.Add(2));
        emitter.prependOnceListener("test", () => order.Add(3));

        emitter.emit("test");

        Assert.Equal(new[] { 3, 1, 2 }, order);
    }

    [Fact]
    public void prependOnceListener_ShouldReturnEmitterForChaining()
    {
        var emitter = new EventEmitter();

        var result = emitter.prependOnceListener("test", () => { });

        Assert.Same(emitter, result);
    }

    [Fact]
    public void prependOnceListener_ShouldRemoveListenerAfterExecution()
    {
        var emitter = new EventEmitter();
        emitter.prependOnceListener("test", () => { });

        emitter.emit("test");
        var count = emitter.listenerCount("test");

        Assert.Equal(0, count);
    }

    [Fact]
    public void prependOnceListener_WithArguments_ShouldPassArguments()
    {
        var emitter = new EventEmitter();
        var receivedValue = 0;

        emitter.prependOnceListener("test", (Action<object?>)(value => receivedValue = (int)value!));
        emitter.emit("test", 42);

        Assert.Equal(42, receivedValue);
    }
}
