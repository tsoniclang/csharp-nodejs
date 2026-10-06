using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class retainedFailureTests
{
    [Fact]
    public void emit_OnceBatchHasNoPerRegistrationAllocation()
    {
        var emitter = new EventEmitter();
        emitter.setMaxListeners(0);
        Action listener = static () => { };
        emitter.once("warm", listener);
        emitter.emit("warm");
        for (var index = 0; index < 1024; index++)
            emitter.once("data", listener);

        var before = GC.GetAllocatedBytesForCurrentThread();
        var dispatched = emitter.emit("data");
        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.True(dispatched);
        Assert.Equal(0, allocated);
        Assert.Equal(0, emitter.listenerCount("data"));
        Assert.Empty(emitter.eventNames());
    }

    [Fact]
    public void emit_QueriesAndMutationsObserveExactLiveRegistrationsDuringCallback()
    {
        var emitter = new EventEmitter();
        Action remaining = static () => { };
        emitter.once("data", () =>
        {
            Assert.Equal(1, emitter.listenerCount("data"));
            Assert.Equal(new Delegate[] { remaining }, emitter.listeners("data"));
            emitter.removeListener("data", remaining);
            Assert.Equal(0, emitter.listenerCount("data"));
            Assert.Empty(emitter.eventNames());
            emitter.once("data", remaining);
        });
        emitter.once("data", remaining);

        Assert.True(emitter.emit("data"));
        Assert.Equal(1, emitter.listenerCount("data"));
        Assert.True(emitter.emit("data"));
        Assert.Equal(0, emitter.listenerCount("data"));
    }

    [Fact]
    public void emit_PreservesOriginalFailureAndPendingOnceListener()
    {
        var emitter = new EventEmitter();
        var original = new InvalidOperationException("original listener");
        var called = 0;
        emitter.once("data", () => throw original);
        emitter.once("data", () => called++);

        Assert.Same(original, Assert.Throws<InvalidOperationException>(() => emitter.emit("data")));
        Assert.Equal(0, called);
        Assert.Equal(1, emitter.listenerCount("data"));
        Assert.True(emitter.emit("data"));
        Assert.Equal(1, called);
        Assert.Equal(0, emitter.listenerCount("data"));
    }

    [Fact]
    public void emit_ReentrantOnceSnapshotsInvokeEachRegistrationOnce()
    {
        var emitter = new EventEmitter();
        var calls = new List<int>();
        emitter.once("data", () =>
        {
            calls.Add(1);
            Assert.True(emitter.emit("data"));
        });
        emitter.once("data", () => calls.Add(2));

        Assert.True(emitter.emit("data"));
        Assert.Equal(new[] { 1, 2 }, calls);
        Assert.False(emitter.emit("data"));
    }

    [Fact]
    public void emit_DoesNotRemoveNewRegistrationWithTheSameDelegate()
    {
        var emitter = new EventEmitter();
        var called = 0;
        Action listener = () => called++;
        emitter.once("data", () =>
        {
            emitter.removeListener("data", listener);
            emitter.once("data", listener);
        });
        emitter.once("data", listener);

        Assert.True(emitter.emit("data"));
        Assert.Equal(1, called);
        Assert.Equal(1, emitter.listenerCount("data"));
        Assert.True(emitter.emit("data"));
        Assert.Equal(2, called);
    }
}
