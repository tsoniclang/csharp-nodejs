using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class removeAllListenersTests
{
    [Fact]
    public void removeAllListeners_NoEventName_ShouldRemoveAll()
    {
        var emitter = new EventEmitter();
        var count1 = 0;
        var count2 = 0;

        emitter.on("event1", () => count1++);
        emitter.on("event2", () => count2++);

        emitter.removeAllListeners();

        emitter.emit("event1");
        emitter.emit("event2");

        Assert.Equal(0, count1);
        Assert.Equal(0, count2);
    }

    [Fact]
    public void removeAllListeners_WithEventName_ShouldRemoveOnlyThatEvent()
    {
        var emitter = new EventEmitter();
        var count1 = 0;
        var count2 = 0;

        emitter.on("event1", () => count1++);
        emitter.on("event2", () => count2++);

        emitter.removeAllListeners("event1");

        emitter.emit("event1");
        emitter.emit("event2");

        Assert.Equal(0, count1);
        Assert.Equal(1, count2);
    }
}
