using System.Linq;
using Tsonic.CSharp.Runtime;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class eventNamesTests
{
    [Fact]
    public void eventNames_ShouldReturnRegisteredEvents()
    {
        var emitter = new EventEmitter();

        emitter.on("event1", () => { });
        emitter.on("event2", () => { });
        emitter.on("event3", () => { });

        var names = emitter.eventNames();

        var textNames = names.Select(name => TsValue.CastDynamic<string>(name)).ToArray();
        Assert.Equal(3, names.length);
        Assert.Contains("event1", textNames);
        Assert.Contains("event2", textNames);
        Assert.Contains("event3", textNames);
    }

    [Fact]
    public void eventNames_NoEvents_ShouldReturnEmptyArray()
    {
        var emitter = new EventEmitter();
        var names = emitter.eventNames();

        Assert.Empty(names);
    }
}
