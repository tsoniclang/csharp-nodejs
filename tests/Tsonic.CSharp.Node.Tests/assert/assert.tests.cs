using Xunit;
using System.Text.RegularExpressions;

namespace Tsonic.CSharp.Node.Tests;

public class AssertTests
{
    [Fact]
    public void ok_WithTrue_ShouldNotThrow()
    {
        assert.ok(true);
    }

    [Fact]
    public void ok_WithFalse_ShouldThrow()
    {
        Assert.Throws<AssertionError>(() => assert.ok(false));
    }

    [Fact]
    public void ok_WithFalse_ShouldThrowWithMessage()
    {
        var ex = Assert.Throws<AssertionError>(() => assert.ok(false, "Test message"));
        Assert.Equal("Test message", ex.Message);
    }

    [Fact]
    public void fail_ShouldAlwaysThrow()
    {
        Assert.Throws<AssertionError>(() => assert.fail());
    }

    [Fact]
    public void fail_WithMessage_ShouldThrowWithMessage()
    {
        var ex = Assert.Throws<AssertionError>(() => assert.fail("Custom failure"));
        Assert.Equal("Custom failure", ex.Message);
    }

    [Fact]
    public void equal_WithEqualValues_ShouldNotThrow()
    {
        assert.equal(5, 5);
        assert.equal("hello", "hello");
    }

    [Fact]
    public void equal_WithDifferentValues_ShouldThrow()
    {
        Assert.Throws<AssertionError>(() => assert.equal(5, 6));
    }

    [Fact]
    public void equal_WithNumericCoercion_ShouldNotThrow()
    {
        assert.equal(5, 5.0);
    }

    [Fact]
    public void notEqual_WithDifferentValues_ShouldNotThrow()
    {
        assert.notEqual(5, 6);
        assert.notEqual("hello", "world");
    }

    [Fact]
    public void notEqual_WithEqualValues_ShouldThrow()
    {
        Assert.Throws<AssertionError>(() => assert.notEqual(5, 5));
    }

    [Fact]
    public void strictEqual_WithEqualValues_ShouldNotThrow()
    {
        assert.strictEqual(5, 5);
        assert.strictEqual("hello", "hello");
    }

    [Fact]
    public void strictEqual_WithDifferentValues_ShouldThrow()
    {
        Assert.Throws<AssertionError>(() => assert.strictEqual(5, 6));
    }

    [Fact]
    public void strictEqual_WithDifferentTypes_ShouldThrow()
    {
        Assert.Throws<AssertionError>(() => assert.strictEqual(5, "5"));
    }

    [Fact]
    public void notStrictEqual_WithDifferentValues_ShouldNotThrow()
    {
        assert.notStrictEqual(5, 6);
        assert.notStrictEqual(5, "5");
    }

    [Fact]
    public void notStrictEqual_WithEqualValues_ShouldThrow()
    {
        Assert.Throws<AssertionError>(() => assert.notStrictEqual(5, 5));
    }

    [Fact]
    public void deepEqual_WithEqualObjects_ShouldNotThrow()
    {
        var obj1 = new { name = "Alice", age = 30 };
        var obj2 = new { name = "Alice", age = 30 };
        assert.deepEqual(obj1, obj2);
    }

    [Fact]
    public void deepEqual_WithDifferentObjects_ShouldThrow()
    {
        var obj1 = new { name = "Alice", age = 30 };
        var obj2 = new { name = "Bob", age = 25 };
        Assert.Throws<AssertionError>(() => assert.deepEqual(obj1, obj2));
    }

    [Fact]
    public void deepEqual_WithNestedObjects_ShouldWork()
    {
        var obj1 = new { user = new { name = "Alice" } };
        var obj2 = new { user = new { name = "Alice" } };
        assert.deepEqual(obj1, obj2);
    }

    [Fact]
    public void notDeepEqual_WithDifferentObjects_ShouldNotThrow()
    {
        var obj1 = new { name = "Alice" };
        var obj2 = new { name = "Bob" };
        assert.notDeepEqual(obj1, obj2);
    }

    [Fact]
    public void notDeepEqual_WithEqualObjects_ShouldThrow()
    {
        var obj1 = new { name = "Alice" };
        var obj2 = new { name = "Alice" };
        Assert.Throws<AssertionError>(() => assert.notDeepEqual(obj1, obj2));
    }

    [Fact]
    public void deepStrictEqual_WithEqualObjects_ShouldNotThrow()
    {
        var obj1 = new { name = "Alice", age = 30 };
        var obj2 = new { name = "Alice", age = 30 };
        assert.deepStrictEqual(obj1, obj2);
    }

    [Fact]
    public void deepStrictEqual_WithDifferentObjects_ShouldThrow()
    {
        var obj1 = new { name = "Alice", age = 30 };
        var obj2 = new { name = "Bob", age = 25 };
        Assert.Throws<AssertionError>(() => assert.deepStrictEqual(obj1, obj2));
    }

    [Fact]
    public void notDeepStrictEqual_WithDifferentObjects_ShouldNotThrow()
    {
        var obj1 = new { name = "Alice" };
        var obj2 = new { name = "Bob" };
        assert.notDeepStrictEqual(obj1, obj2);
    }

    [Fact]
    public void notDeepStrictEqual_WithEqualObjects_ShouldThrow()
    {
        var obj1 = new { name = "Alice" };
        var obj2 = new { name = "Alice" };
        Assert.Throws<AssertionError>(() => assert.notDeepStrictEqual(obj1, obj2));
    }

    [Fact]
    public void throws_WhenFunctionThrows_ShouldNotThrow()
    {
        assert.throws(() => throw new System.Exception("Test error"));
    }

    [Fact]
    public void throws_WhenFunctionDoesNotThrow_ShouldThrow()
    {
        Assert.Throws<AssertionError>(() => assert.throws(() => { }));
    }

    [Fact]
    public void throws_WithMessage_ShouldIncludeMessage()
    {
        var ex = Assert.Throws<AssertionError>(() => assert.throws(() => { }, "Should have thrown"));
        Assert.Equal("Should have thrown", ex.Message);
    }

    [Fact]
    public void doesNotThrow_WhenFunctionDoesNotThrow_ShouldNotThrow()
    {
        assert.doesNotThrow(() => { });
    }

    [Fact]
    public void doesNotThrow_WhenFunctionThrows_ShouldThrow()
    {
        Assert.Throws<AssertionError>(() =>
            assert.doesNotThrow(() => throw new System.Exception("Error")));
    }

    [Fact]
    public void match_WithMatchingString_ShouldNotThrow()
    {
        assert.match("hello world", new Regex("world"));
    }

    [Fact]
    public void match_WithNonMatchingString_ShouldThrow()
    {
        Assert.Throws<AssertionError>(() =>
            assert.match("hello world", new Regex("goodbye")));
    }

    [Fact]
    public void match_WithComplexPattern_ShouldWork()
    {
        assert.match("test123", new Regex(@"\d+"));
    }

    [Fact]
    public void doesNotMatch_WithNonMatchingString_ShouldNotThrow()
    {
        assert.doesNotMatch("hello world", new Regex("goodbye"));
    }

    [Fact]
    public void doesNotMatch_WithMatchingString_ShouldThrow()
    {
        Assert.Throws<AssertionError>(() =>
            assert.doesNotMatch("hello world", new Regex("world")));
    }

    [Fact]
    public void ifError_WithNull_ShouldNotThrow()
    {
        assert.ifError(null);
    }

    [Fact]
    public void ifError_WithNonNull_ShouldThrow()
    {
        Assert.Throws<AssertionError>(() => assert.ifError("error"));
    }

    [Fact]
    public void ifError_WithException_ShouldThrowException()
    {
        var exception = new System.Exception("Test error");
        var thrown = Assert.Throws<System.Exception>(() => assert.ifError(exception));
        Assert.Same(exception, thrown);
    }

    [Fact]
    public void AssertionError_ShouldHaveCorrectProperties()
    {
        var err = new AssertionError("Test message", 5, 10, "===");
        Assert.Equal("Test message", err.Message);
        Assert.Equal(5, err.actual);
        Assert.Equal(10, err.expected);
        Assert.Equal("===", err.@operator);
        Assert.False(err.generatedMessage);
        Assert.Equal("ERR_ASSERTION", err.code);
    }

    [Fact]
    public void AssertionError_WithoutMessage_ShouldGenerateMessage()
    {
        var err = new AssertionError(null, 5, 10, "===");
        Assert.NotEmpty(err.Message);
        Assert.True(err.generatedMessage);
    }
}
