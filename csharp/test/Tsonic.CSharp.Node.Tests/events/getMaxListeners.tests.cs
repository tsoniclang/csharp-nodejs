using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class getMaxListenersTests
{
    [Fact]
    public void getMaxListeners_ShouldReturnDefaultValue()
    {
        var emitter = new EventEmitter();

        var max = emitter.getMaxListeners();

        Assert.Equal(10, max);
    }

    [Fact]
    public void getMaxListeners_ShouldReturnSetValue()
    {
        var emitter = new EventEmitter();
        emitter.setMaxListeners(20);

        var max = emitter.getMaxListeners();

        Assert.Equal(20, max);
    }

    [Fact]
    public void getMaxListeners_ShouldReturnZeroWhenSetToZero()
    {
        var emitter = new EventEmitter();
        emitter.setMaxListeners(0);

        var max = emitter.getMaxListeners();

        Assert.Equal(0, max);
    }
}
