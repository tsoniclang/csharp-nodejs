using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class newListenerTests
{
    [Fact]
    public void newListenerEvent_ShouldBeEmitted()
    {
        var emitter = new EventEmitter();
        string? eventName = null;

        emitter.on("newListener", (Action<object?, object?>)((name, listener) =>
        {
            eventName = name as string;
        }));

        emitter.on("test", () => { });

        Assert.Equal("test", eventName);
    }
}
