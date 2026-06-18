using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class onTests
{
    [Fact]
    public void on_ShouldRegisterListener()
    {
        var emitter = new EventEmitter();
        var called = false;

        emitter.on("test", () => called = true);
        emitter.emit("test");

        Assert.True(called);
    }

    [Fact]
    public void on_MultipleListeners_ShouldCallAllListeners()
    {
        var emitter = new EventEmitter();
        var count = 0;

        emitter.on("test", () => count++);
        emitter.on("test", () => count++);
        emitter.on("test", () => count++);
        emitter.emit("test");

        Assert.Equal(3, count);
    }

    [Fact]
    public void on_WithArguments_ShouldPassArguments()
    {
        var emitter = new EventEmitter();
        object? receivedArg = null;

        emitter.on("test", (Action<object?>)(arg => receivedArg = arg));
        emitter.emit("test", 42);

        Assert.Equal(42, receivedArg);
    }

    [Fact]
    public void on_ShouldReturnThis()
    {
        var emitter = new EventEmitter();

        var result = emitter.on("test", () => { });

        Assert.Same(emitter, result);
    }

    [Fact]
    public void methodChaining_ShouldWork()
    {
        var emitter = new EventEmitter();
        var count = 0;

        emitter
            .on("test", () => count++)
            .on("test", () => count++)
            .setMaxListeners(5);

        emitter.emit("test");

        Assert.Equal(2, count);
        Assert.Equal(5, emitter.getMaxListeners());
    }
}
