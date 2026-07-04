using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class emitTests
{
    [Fact]
    public void emit_NoListeners_ShouldReturnFalse()
    {
        var emitter = new EventEmitter();
        var result = emitter.emit("test");

        Assert.False(result);
    }

    [Fact]
    public void emit_WithListeners_ShouldReturnTrue()
    {
        var emitter = new EventEmitter();
        emitter.on("test", () => { });

        var result = emitter.emit("test");

        Assert.True(result);
    }

    [Fact]
    public void emit_ErrorEvent_NoListeners_ShouldThrow()
    {
        var emitter = new EventEmitter();

        Assert.Throws<Exception>(() => emitter.emit("error", new Exception("Test error")));
    }

    [Fact]
    public void emit_ErrorEvent_WithListeners_ShouldNotThrow()
    {
        var emitter = new EventEmitter();
        Exception? caughtError = null;

        emitter.on("error", (Action<object?>)(err => caughtError = err as Exception));
        emitter.emit("error", new Exception("Test error"));

        Assert.NotNull(caughtError);
        Assert.Equal("Test error", caughtError.Message);
    }

    [Fact]
    public void emit_MultipleArguments_ShouldAllBePassedToListeners()
    {
        var emitter = new EventEmitter();
        object?[] received = null!;

        emitter.on("test", (Action<object?[]>)(args => received = args));
        emitter.emit("test", "arg1", 42, true);

        Assert.Equal(3, received.Length);
        Assert.Equal("arg1", received[0]);
        Assert.Equal(42, received[1]);
        Assert.Equal(true, received[2]);
    }
}
