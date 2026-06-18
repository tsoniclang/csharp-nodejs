using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class utilTests
{
    [Fact]
    public void format_ShouldReturnEmptyStringForNull()
    {
        var result = util.format(null);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void format_ShouldReturnStringAsIs()
    {
        var result = util.format("Hello World");
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void format_ShouldFormatStringPlaceholder()
    {
        var result = util.format("Hello %s", "World");
        Assert.Equal("Hello World", result);
    }

    [Fact]
    public void format_ShouldFormatNumberPlaceholder()
    {
        var result = util.format("Count: %d", 42);
        Assert.Equal("Count: 42", result);
    }

    [Fact]
    public void format_ShouldFormatMultiplePlaceholders()
    {
        var result = util.format("%s is %d years old", "Alice", 30);
        Assert.Equal("Alice is 30 years old", result);
    }

    [Fact]
    public void format_ShouldHandleLiteralPercent()
    {
        var result = util.format("100%% complete");
        Assert.Equal("100% complete", result);
    }

    [Fact]
    public void format_ShouldAppendExtraArguments()
    {
        var result = util.format("Hello", "World", "!");
        Assert.Equal("Hello World !", result);
    }

    [Fact]
    public void format_ShouldFormatJsonPlaceholder()
    {
        var result = util.format("Data: %j", new { name = "Alice", age = 30 });
        Assert.Contains("Alice", result);
        Assert.Contains("30", result);
    }

    [Fact]
    public void inspect_ShouldReturnNullForNull()
    {
        var result = util.inspect(null);
        Assert.Equal("null", result);
    }

    [Fact]
    public void inspect_ShouldFormatString()
    {
        var result = util.inspect("Hello");
        Assert.Equal("'Hello'", result);
    }

    [Fact]
    public void inspect_ShouldFormatBoolean()
    {
        var result1 = util.inspect(true);
        var result2 = util.inspect(false);
        Assert.Equal("true", result1);
        Assert.Equal("false", result2);
    }

    [Fact]
    public void inspect_ShouldFormatNumber()
    {
        var result = util.inspect(42);
        Assert.Equal("42", result);
    }

    [Fact]
    public void inspect_ShouldFormatObject()
    {
        var result = util.inspect(new { name = "Alice", age = 30 });
        Assert.Contains("Alice", result);
        Assert.Contains("30", result);
    }

    [Fact]
    public void isArray_ShouldReturnTrueForArray()
    {
        var arr = new int[] { 1, 2, 3 };
        Assert.True(util.isArray(arr));
    }

    [Fact]
    public void isArray_ShouldReturnFalseForNonArray()
    {
        Assert.False(util.isArray("string"));
        Assert.False(util.isArray(42));
        Assert.False(util.isArray(new { }));
    }

    [Fact]
    public void isArray_ShouldReturnFalseForNull()
    {
        Assert.False(util.isArray(null));
    }

    [Fact]
    public void isDeepStrictEqual_ShouldReturnTrueForNulls()
    {
        Assert.True(util.isDeepStrictEqual(null, null));
    }

    [Fact]
    public void isDeepStrictEqual_ShouldReturnFalseForNullAndNonNull()
    {
        Assert.False(util.isDeepStrictEqual(null, 42));
        Assert.False(util.isDeepStrictEqual(42, null));
    }

    [Fact]
    public void isDeepStrictEqual_ShouldReturnTrueForSameReference()
    {
        var obj = new { name = "Alice" };
        Assert.True(util.isDeepStrictEqual(obj, obj));
    }

    [Fact]
    public void isDeepStrictEqual_ShouldReturnTrueForEqualPrimitives()
    {
        Assert.True(util.isDeepStrictEqual(42, 42));
        Assert.True(util.isDeepStrictEqual("hello", "hello"));
        Assert.True(util.isDeepStrictEqual(true, true));
    }

    [Fact]
    public void isDeepStrictEqual_ShouldReturnFalseForDifferentPrimitives()
    {
        Assert.False(util.isDeepStrictEqual(42, 43));
        Assert.False(util.isDeepStrictEqual("hello", "world"));
        Assert.False(util.isDeepStrictEqual(true, false));
    }

    [Fact]
    public void isDeepStrictEqual_ShouldReturnFalseForDifferentTypes()
    {
        Assert.False(util.isDeepStrictEqual(42, "42"));
    }

    [Fact]
    public void inherits_ShouldNotThrow()
    {
        // inherits is a no-op in C#, just verify it doesn't throw
        util.inherits(null, null);
        util.inherits(new object(), new object());
    }

    [Fact]
    public void debuglog_ShouldReturnFunction()
    {
        var debug = util.debuglog("test");
        Assert.NotNull(debug);

        // Should not throw when called
        debug("test message");
        debug("test message with args: {0}", 42);
    }

    [Fact]
    public void debuglog_ShouldBeNoOpWhenNotEnabled()
    {
        // Without NODE_DEBUG set, should be a no-op
        var originalNodeDebug = Environment.GetEnvironmentVariable("NODE_DEBUG");
        Environment.SetEnvironmentVariable("NODE_DEBUG", "");

        var debug = util.debuglog("test");
        debug("This should not appear");

        // Restore original
        Environment.SetEnvironmentVariable("NODE_DEBUG", originalNodeDebug);
    }

    [Fact]
    public void deprecate_ShouldWrapFunction()
    {
        var called = false;
        Func<int> fn = () => { called = true; return 42; };

        var deprecated = util.deprecate(fn, "This is deprecated");
        var result = deprecated();

        Assert.True(called);
        Assert.Equal(42, result);
    }

    [Fact]
    public void deprecate_ShouldWrapAction()
    {
        var called = false;
        Action action = () => { called = true; };

        var deprecated = util.deprecate(action, "This is deprecated");
        deprecated();

        Assert.True(called);
    }
}
