using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class utilTests
{
    [Fact]
    public void format_ShouldRejectOpenCarrierSemantics()
    {
        var exception = Assert.Throws<NotSupportedException>(() => util.format("Hello %s", "World"));
        Assert.Contains("node:util.format", exception.Message);
        Assert.Contains("closed provider/runtime carrier semantics", exception.Message);
    }

    [Fact]
    public void inspect_ShouldRejectOpenCarrierSemantics()
    {
        var exception = Assert.Throws<NotSupportedException>(() => util.inspect(new { name = "Alice", age = 30 }));
        Assert.Contains("node:util.inspect", exception.Message);
        Assert.Contains("closed provider/runtime carrier semantics", exception.Message);
    }

    [Fact]
    public void isArray_ShouldRejectOpenCarrierSemantics()
    {
        var arr = new int[] { 1, 2, 3 };
        var exception = Assert.Throws<NotSupportedException>(() => util.isArray(arr));
        Assert.Contains("node:util.isArray", exception.Message);
        Assert.Contains("closed provider/runtime carrier semantics", exception.Message);
    }

    [Fact]
    public void isDeepStrictEqual_ShouldRejectOpenCarrierSemantics()
    {
        var exception = Assert.Throws<NotSupportedException>(() => util.isDeepStrictEqual(new { value = 1 }, new { value = 1 }));
        Assert.Contains("node:util.isDeepStrictEqual", exception.Message);
        Assert.Contains("closed provider/runtime carrier semantics", exception.Message);
    }

    [Fact]
    public void inherits_ShouldNotThrow()
    {
        util.inherits(null, null);
        util.inherits(new object(), new object());
    }

    [Fact]
    public void debuglog_ShouldRejectOpenCarrierSemantics()
    {
        var exception = Assert.Throws<NotSupportedException>(() => util.debuglog("test"));
        Assert.Contains("node:util.debuglog", exception.Message);
        Assert.Contains("closed provider/runtime carrier semantics", exception.Message);
    }

    [Fact]
    public void deprecate_ShouldRejectFunctionOpenCarrierSemantics()
    {
        Func<int> fn = () => 42;
        var exception = Assert.Throws<NotSupportedException>(() => util.deprecate(fn, "This is deprecated"));
        Assert.Contains("node:util.deprecate", exception.Message);
        Assert.Contains("closed provider/runtime carrier semantics", exception.Message);
    }

    [Fact]
    public void deprecate_ShouldRejectActionOpenCarrierSemantics()
    {
        Action action = () => { };
        var exception = Assert.Throws<NotSupportedException>(() => util.deprecate(action, "This is deprecated"));
        Assert.Contains("node:util.deprecate", exception.Message);
        Assert.Contains("closed provider/runtime carrier semantics", exception.Message);
    }
}
