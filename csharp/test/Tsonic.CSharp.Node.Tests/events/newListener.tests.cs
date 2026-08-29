using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class newListenerTests
{
    [Fact]
    public void newListenerEvent_ShouldBeEmitted()
    {
        var emitter = new EventEmitter();
        string? eventName = null;

        emitter.on("newListener", (Action<TsValue, Delegate>)((name, listener) =>
        {
            eventName = TsValue.CastDynamic<string>(name);
        }));

        emitter.on("test", () => { });

        Assert.Equal("test", eventName);
    }
}
