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

        Assert.Equal(3, names.Length);
        Assert.Contains("event1", names);
        Assert.Contains("event2", names);
        Assert.Contains("event3", names);
    }

    [Fact]
    public void eventNames_NoEvents_ShouldReturnEmptyArray()
    {
        var emitter = new EventEmitter();
        var names = emitter.eventNames();

        Assert.Empty(names);
    }
}
