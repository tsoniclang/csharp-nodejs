using System.Collections.Generic;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class QueryStringTests
{
    [Fact]
    public void stringify_ShouldSerializeSimpleObject()
    {
        var obj = new Dictionary<string, object?>
        {
            { "foo", "bar" },
            { "baz", "qux" }
        };

        var result = querystring.stringify(obj);

        Assert.Contains("foo=bar", result);
        Assert.Contains("baz=qux", result);
        Assert.Contains("&", result);
    }

    [Fact]
    public void stringify_ShouldHandleArrayValues()
    {
        var obj = new Dictionary<string, object?>
        {
            { "foo", "bar" },
            { "baz", new[] { "qux", "quux" } }
        };

        var result = querystring.stringify(obj);

        Assert.Contains("foo=bar", result);
        Assert.Contains("baz=qux", result);
        Assert.Contains("baz=quux", result);
    }

    [Fact]
    public void stringify_ShouldHandleEmptyObject()
    {
        var obj = new Dictionary<string, object?>();

        var result = querystring.stringify(obj);

        Assert.Equal("", result);
    }

    [Fact]
    public void stringify_ShouldHandleNullObject()
    {
        var result = querystring.stringify(null);

        Assert.Equal("", result);
    }

    [Fact]
    public void stringify_ShouldUseCustomSeparators()
    {
        var obj = new Dictionary<string, object?>
        {
            { "foo", "bar" },
            { "baz", "qux" }
        };

        var result = querystring.stringify(obj, ";", ":");

        Assert.Contains("foo:bar", result);
        Assert.Contains("baz:qux", result);
        Assert.Contains(";", result);
    }

    [Fact]
    public void stringify_ShouldEscapeSpecialCharacters()
    {
        var obj = new Dictionary<string, object?>
        {
            { "key with spaces", "value with spaces" },
            { "special", "hello&world" }
        };

        var result = querystring.stringify(obj);

        Assert.DoesNotContain(" ", result);
        Assert.Contains("key%20with%20spaces", result);
    }

    [Fact]
    public void parse_ShouldParseSimpleQueryString()
    {
        var result = querystring.parse("foo=bar&baz=qux");

        Assert.Equal("bar", result["foo"]);
        Assert.Equal("qux", result["baz"]);
    }

    [Fact]
    public void parse_ShouldHandleMultipleValuesForSameKey()
    {
        var result = querystring.parse("foo=bar&foo=baz");

        Assert.IsType<string[]>(result["foo"]);
        var values = (string[])result["foo"];
        Assert.Equal(2, values.Length);
        Assert.Contains("bar", values);
        Assert.Contains("baz", values);
    }

    [Fact]
    public void parse_ShouldHandleEmptyString()
    {
        var result = querystring.parse("");

        Assert.Empty(result);
    }

    [Fact]
    public void parse_ShouldHandleLeadingQuestionMark()
    {
        var result = querystring.parse("?foo=bar&baz=qux");

        Assert.Equal("bar", result["foo"]);
        Assert.Equal("qux", result["baz"]);
    }

    [Fact]
    public void parse_ShouldHandleCustomSeparators()
    {
        var result = querystring.parse("foo:bar;baz:qux", ";", ":");

        Assert.Equal("bar", result["foo"]);
        Assert.Equal("qux", result["baz"]);
    }

    [Fact]
    public void parse_ShouldUnescapeSpecialCharacters()
    {
        var result = querystring.parse("key%20with%20spaces=value%20with%20spaces");

        Assert.Equal("value with spaces", result["key with spaces"]);
    }

    [Fact]
    public void parse_ShouldRespectMaxKeys()
    {
        var result = querystring.parse("a=1&b=2&c=3&d=4", maxKeys: 2);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void parse_ShouldHandleMaxKeysZero()
    {
        var result = querystring.parse("a=1&b=2&c=3&d=4", maxKeys: 0);

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void parse_ShouldHandleKeyWithoutValue()
    {
        var result = querystring.parse("foo=bar&baz&qux=quux");

        Assert.Equal("bar", result["foo"]);
        Assert.Equal("", result["baz"]);
        Assert.Equal("quux", result["qux"]);
    }

    [Fact]
    public void encode_ShouldBeAliasForStringify()
    {
        var obj = new Dictionary<string, object?>
        {
            { "foo", "bar" }
        };

        var stringifyResult = querystring.stringify(obj);
        var encodeResult = querystring.encode(obj);

        Assert.Equal(stringifyResult, encodeResult);
    }

    [Fact]
    public void decode_ShouldBeAliasForParse()
    {
        var str = "foo=bar&baz=qux";

        var parseResult = querystring.parse(str);
        var decodeResult = querystring.decode(str);

        Assert.Equal(parseResult.Count, decodeResult.Count);
        Assert.Equal(parseResult["foo"], decodeResult["foo"]);
    }

    [Fact]
    public void escape_ShouldPercentEncodeString()
    {
        var result = querystring.escape("hello world");

        Assert.Equal("hello%20world", result);
    }

    [Fact]
    public void escape_ShouldHandleSpecialCharacters()
    {
        var result = querystring.escape("hello&world=test");

        Assert.DoesNotContain("&", result);
        Assert.DoesNotContain("=", result);
        Assert.Contains("%26", result);
        Assert.Contains("%3D", result);
    }

    [Fact]
    public void unescape_ShouldDecodePercentEncodedString()
    {
        var result = querystring.unescape("hello%20world");

        Assert.Equal("hello world", result);
    }

    [Fact]
    public void unescape_ShouldHandleMalformedString()
    {
        var result = querystring.unescape("hello%world");

        // Should not throw, returns original or partially decoded
        Assert.NotNull(result);
    }

    [Fact]
    public void roundTrip_ShouldPreserveData()
    {
        var original = new Dictionary<string, object?>
        {
            { "name", "John Doe" },
            { "email", "john@example.com" },
            { "tags", new[] { "developer", "designer" } }
        };

        var encoded = querystring.stringify(original);
        var decoded = querystring.parse(encoded);

        Assert.Equal("John Doe", decoded["name"]);
        Assert.Equal("john@example.com", decoded["email"]);
        Assert.IsType<string[]>(decoded["tags"]);
        var tags = (string[])decoded["tags"];
        Assert.Equal(2, tags.Length);
    }
}
