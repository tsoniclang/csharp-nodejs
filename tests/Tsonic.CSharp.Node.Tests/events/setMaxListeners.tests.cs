using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class setMaxListenersTests
{
    [Fact]
    public void setMaxListeners_ShouldSetLimit()
    {
        var emitter = new EventEmitter();

        emitter.setMaxListeners(5);

        Assert.Equal(5, emitter.getMaxListeners());
    }

    [Fact]
    public void setMaxListeners_NegativeValue_ShouldThrow()
    {
        var emitter = new EventEmitter();

        Assert.Throws<ArgumentException>(() => emitter.setMaxListeners(-1));
    }

    [Fact]
    public void getMaxListeners_DefaultValue_ShouldBe10()
    {
        var emitter = new EventEmitter();

        Assert.Equal(10, emitter.getMaxListeners());
    }

    [Fact]
    public void defaultMaxListeners_ShouldBe10()
    {
        Assert.Equal(10, EventEmitter.defaultMaxListeners);
    }

    [Fact]
    public void defaultMaxListeners_CanBeModified()
    {
        var original = EventEmitter.defaultMaxListeners;

        EventEmitter.defaultMaxListeners = 20;
        Assert.Equal(20, EventEmitter.defaultMaxListeners);

        // Restore original
        EventEmitter.defaultMaxListeners = original;
    }
}
