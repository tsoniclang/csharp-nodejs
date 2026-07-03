using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class listenerCountTests
{
    [Fact]
    public void listenerCount_ShouldReturnCorrectCount()
    {
        var emitter = new EventEmitter();

        emitter.on("test", () => { });
        emitter.on("test", () => { });
        emitter.on("test", () => { });

        Assert.Equal(3, emitter.listenerCount("test"));
    }

    [Fact]
    public void listenerCount_NoListeners_ShouldReturnZero()
    {
        var emitter = new EventEmitter();
        Assert.Equal(0, emitter.listenerCount("test"));
    }
}
