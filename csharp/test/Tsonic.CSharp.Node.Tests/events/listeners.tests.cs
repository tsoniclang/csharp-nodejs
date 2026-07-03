using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class listenersTests
{
    [Fact]
    public void listeners_ShouldReturnListenerArray()
    {
        var emitter = new EventEmitter();
        Action listener1 = () => { };
        Action listener2 = () => { };

        emitter.on("test", listener1);
        emitter.on("test", listener2);

        var listeners = emitter.listeners("test");

        Assert.Equal(2, listeners.Length);
    }

    [Fact]
    public void listeners_NoListeners_ShouldReturnEmptyArray()
    {
        var emitter = new EventEmitter();
        var listeners = emitter.listeners("test");

        Assert.Empty(listeners);
    }

    [Fact]
    public void rawListeners_ShouldReturnListeners()
    {
        var emitter = new EventEmitter();
        Action listener = () => { };

        emitter.on("test", listener);

        var rawListeners = emitter.rawListeners("test");

        Assert.Single(rawListeners);
    }
}
