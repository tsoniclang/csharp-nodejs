using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class onceTests
{
    [Fact]
    public void once_ShouldCallListenerOnlyOnce()
    {
        var emitter = new EventEmitter();
        var count = 0;

        emitter.once("test", () => count++);
        emitter.emit("test");
        emitter.emit("test");
        emitter.emit("test");

        Assert.Equal(1, count);
    }
}
