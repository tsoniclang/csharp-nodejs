using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class rawListenersTests
{
    [Fact]
    public void rawListeners_ShouldReturnCopyOfListeners()
    {
        var emitter = new EventEmitter();
        Action listener1 = () => { };
        Action listener2 = () => { };

        emitter.on("test", listener1);
        emitter.on("test", listener2);

        var listeners = emitter.rawListeners("test");

        Assert.Equal(2, listeners.Length);
    }

    [Fact]
    public void rawListeners_NonExistentEvent_ShouldReturnEmptyArray()
    {
        var emitter = new EventEmitter();

        var listeners = emitter.rawListeners("nonexistent");

        Assert.Empty(listeners);
    }

    [Fact]
    public void rawListeners_ShouldReturnArrayCopy()
    {
        var emitter = new EventEmitter();
        Action listener = () => { };
        emitter.on("test", listener);

        var listeners1 = emitter.rawListeners("test");
        var listeners2 = emitter.rawListeners("test");

        Assert.NotSame(listeners1, listeners2);
    }

    [Fact]
    public void rawListeners_WithOnceListeners_ShouldIncludeWrappers()
    {
        var emitter = new EventEmitter();
        Action listener = () => { };

        emitter.once("test", listener);

        var listeners = emitter.rawListeners("test");

        Assert.Single(listeners);
    }
}
