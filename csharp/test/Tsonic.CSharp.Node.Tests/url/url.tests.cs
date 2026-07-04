using Xunit;
using System.Linq;

namespace Tsonic.CSharp.Node.Tests;

public class URLTests
{
    [Fact]
    public void URL_Constructor_ShouldParseSimpleURL()
    {
        var url = new URL("https://example.com/path");
        Assert.Equal("https://example.com/path", url.href);
    }

    [Fact]
    public void URL_Constructor_WithBase_ShouldResolveRelativeURL()
    {
        var url = new URL("/path", "https://example.com");
        Assert.Equal("https://example.com/path", url.href);
    }

    [Fact]
    public void URL_protocol_ShouldReturnProtocolWithColon()
    {
        var url = new URL("https://example.com");
        Assert.Equal("https:", url.protocol);
    }

    [Fact]
    public void URL_protocol_Setter_ShouldChangeProtocol()
    {
        var url = new URL("https://example.com");
        url.protocol = "http:";
        Assert.StartsWith("http://example.com", url.href);
    }

    [Fact]
    public void URL_hostname_ShouldReturnHostname()
    {
        var url = new URL("https://example.com:8080");
        Assert.Equal("example.com", url.hostname);
    }

    [Fact]
    public void URL_hostname_Setter_ShouldChangeHostname()
    {
        var url = new URL("https://example.com");
        url.hostname = "test.com";
        Assert.Equal("test.com", url.hostname);
    }

    [Fact]
    public void URL_port_ShouldReturnPort()
    {
        var url = new URL("https://example.com:8080");
        Assert.Equal("8080", url.port);
    }

    [Fact]
    public void URL_port_WithDefaultPort_ShouldReturnEmpty()
    {
        var url = new URL("https://example.com:443");
        Assert.Equal("", url.port);
    }

    [Fact]
    public void URL_port_Setter_ShouldChangePort()
    {
        var url = new URL("https://example.com");
        url.port = "8080";
        Assert.Contains(":8080", url.href);
    }

    [Fact]
    public void URL_host_ShouldReturnHostnameAndPort()
    {
        var url = new URL("https://example.com:8080");
        Assert.Equal("example.com:8080", url.host);
    }

    [Fact]
    public void URL_host_WithDefaultPort_ShouldReturnHostnameOnly()
    {
        var url = new URL("https://example.com:443");
        Assert.Equal("example.com", url.host);
    }

    [Fact]
    public void URL_pathname_ShouldReturnPath()
    {
        var url = new URL("https://example.com/path/to/resource");
        Assert.Equal("/path/to/resource", url.pathname);
    }

    [Fact]
    public void URL_pathname_Setter_ShouldChangePath()
    {
        var url = new URL("https://example.com/old");
        url.pathname = "/new/path";
        Assert.Equal("/new/path", url.pathname);
    }

    [Fact]
    public void URL_search_ShouldReturnQueryString()
    {
        var url = new URL("https://example.com?foo=bar&baz=qux");
        Assert.Equal("?foo=bar&baz=qux", url.search);
    }

    [Fact]
    public void URL_search_Setter_ShouldChangeQueryString()
    {
        var url = new URL("https://example.com");
        url.search = "?key=value";
        Assert.Equal("?key=value", url.search);
    }

    [Fact]
    public void URL_hash_ShouldReturnFragment()
    {
        var url = new URL("https://example.com#section");
        Assert.Equal("#section", url.hash);
    }

    [Fact]
    public void URL_hash_Setter_ShouldChangeFragment()
    {
        var url = new URL("https://example.com");
        url.hash = "#new-section";
        Assert.Equal("#new-section", url.hash);
    }

    [Fact]
    public void URL_username_ShouldReturnUsername()
    {
        var url = new URL("https://user:pass@example.com");
        Assert.Equal("user", url.username);
    }

    [Fact]
    public void URL_password_ShouldReturnPassword()
    {
        var url = new URL("https://user:pass@example.com");
        Assert.Equal("pass", url.password);
    }

    [Fact]
    public void URL_origin_ShouldReturnOrigin()
    {
        var url = new URL("https://example.com:8080/path");
        Assert.Equal("https://example.com:8080", url.origin);
    }

    [Fact]
    public void URL_searchParams_ShouldReturnURLSearchParams()
    {
        var url = new URL("https://example.com?foo=bar");
        Assert.NotNull(url.searchParams);
        Assert.IsType<URLSearchParams>(url.searchParams);
        Assert.Equal("bar", url.searchParams.get("foo"));
    }

    [Fact]
    public void URL_searchParams_ShouldRemainLiveWithURLSearch()
    {
        var url = new URL("https://example.com/path?foo=bar");
        var searchParams = url.searchParams;

        searchParams.append("baz", "qux");
        Assert.Equal("?foo=bar&baz=qux", url.search);
        Assert.Equal("https://example.com/path?foo=bar&baz=qux", url.href);

        url.search = "?answer=42";
        Assert.Same(searchParams, url.searchParams);
        Assert.False(searchParams.has("foo"));
        Assert.Equal("42", searchParams.get("answer"));

        url.href = "https://example.com/next?live=yes";
        Assert.Same(searchParams, url.searchParams);
        Assert.Equal("yes", searchParams.get("live"));

        searchParams.delete("answer");
        searchParams.delete("live");
        Assert.Equal("", url.search);
        Assert.Equal("https://example.com/next", url.href);
    }

    [Fact]
    public void URL_toString_ShouldReturnSerializedURL()
    {
        var url = new URL("https://example.com/path");
        Assert.Equal(url.href, url.ToString());
    }

    [Fact]
    public void URL_toJSON_ShouldReturnSerializedURL()
    {
        var url = new URL("https://example.com/path");
        Assert.Equal(url.href, url.toJSON());
    }

    [Fact]
    public void URL_canParse_WithValidURL_ShouldReturnTrue()
    {
        Assert.True(URL.canParse("https://example.com"));
    }

    [Fact]
    public void URL_canParse_WithInvalidURL_ShouldReturnFalse()
    {
        Assert.False(URL.canParse("not a url"));
    }

    [Fact]
    public void URL_canParse_WithRelativeURLAndBase_ShouldReturnTrue()
    {
        Assert.True(URL.canParse("/path", "https://example.com"));
    }

    [Fact]
    public void URL_parse_WithValidURL_ShouldReturnURL()
    {
        var url = URL.parse("https://example.com");
        Assert.NotNull(url);
        Assert.Equal("https://example.com/", url!.href);
    }

    [Fact]
    public void URL_parse_WithInvalidURL_ShouldReturnNull()
    {
        var url = URL.parse("not a url");
        Assert.Null(url);
    }
}

public class URLSearchParamsTests
{
    [Fact]
    public void URLSearchParams_Constructor_Empty_ShouldCreateEmpty()
    {
        var params_ = new URLSearchParams();
        Assert.Equal(0, params_.size);
    }

    [Fact]
    public void URLSearchParams_Constructor_WithString_ShouldParse()
    {
        var params_ = new URLSearchParams("foo=bar&baz=qux");
        Assert.Equal(2, params_.size);
        Assert.Equal("bar", params_.get("foo"));
        Assert.Equal("qux", params_.get("baz"));
    }

    [Fact]
    public void URLSearchParams_Constructor_WithQueryString_ShouldParseWithoutQuestionMark()
    {
        var params_ = new URLSearchParams("?foo=bar");
        Assert.Equal("bar", params_.get("foo"));
    }

    [Fact]
    public void URLSearchParams_append_ShouldAddParameter()
    {
        var params_ = new URLSearchParams();
        params_.append("key", "value");
        Assert.Equal("value", params_.get("key"));
    }

    [Fact]
    public void URLSearchParams_append_AllowsDuplicates()
    {
        var params_ = new URLSearchParams();
        params_.append("key", "value1");
        params_.append("key", "value2");
        var values = params_.getAll("key");
        Assert.Equal(2, values.Length);
        Assert.Equal("value1", values[0]);
        Assert.Equal("value2", values[1]);
    }

    [Fact]
    public void URLSearchParams_set_ShouldReplaceExistingValues()
    {
        var params_ = new URLSearchParams("key=old");
        params_.set("key", "new");
        Assert.Equal("new", params_.get("key"));
        Assert.Single(params_.getAll("key"));
    }

    [Fact]
    public void URLSearchParams_set_ShouldPreserveFirstPairOrderAndRemoveDuplicates()
    {
        var params_ = new URLSearchParams("a=1&b=2&a=3&c=4");
        params_.set("a", "5");
        Assert.Equal("a=5&b=2&c=4", params_.ToString());
        Assert.Equal(new[] { "5" }, params_.getAll("a"));
    }

    [Fact]
    public void URLSearchParams_get_WithNonExistentKey_ShouldReturnNull()
    {
        var params_ = new URLSearchParams();
        Assert.Null(params_.get("nonexistent"));
    }

    [Fact]
    public void URLSearchParams_getAll_ShouldReturnAllValues()
    {
        var params_ = new URLSearchParams("key=1&key=2&key=3");
        var values = params_.getAll("key");
        Assert.Equal(3, values.Length);
        Assert.Equal(new[] { "1", "2", "3" }, values);
    }

    [Fact]
    public void URLSearchParams_getAll_WithNonExistentKey_ShouldReturnEmptyArray()
    {
        var params_ = new URLSearchParams();
        Assert.Empty(params_.getAll("nonexistent"));
    }

    [Fact]
    public void URLSearchParams_has_WithExistingKey_ShouldReturnTrue()
    {
        var params_ = new URLSearchParams("key=value");
        Assert.True(params_.has("key"));
    }

    [Fact]
    public void URLSearchParams_has_WithNonExistentKey_ShouldReturnFalse()
    {
        var params_ = new URLSearchParams();
        Assert.False(params_.has("key"));
    }

    [Fact]
    public void URLSearchParams_has_WithValue_ShouldCheckBothKeyAndValue()
    {
        var params_ = new URLSearchParams("key=value1&key=value2");
        Assert.True(params_.has("key", "value1"));
        Assert.False(params_.has("key", "value3"));
    }

    [Fact]
    public void URLSearchParams_delete_ShouldRemoveParameter()
    {
        var params_ = new URLSearchParams("key=value");
        params_.delete("key");
        Assert.False(params_.has("key"));
    }

    [Fact]
    public void URLSearchParams_delete_WithValue_ShouldRemoveSpecificPair()
    {
        var params_ = new URLSearchParams("key=value1&key=value2");
        params_.delete("key", "value1");
        Assert.Single(params_.getAll("key"));
        Assert.Equal("value2", params_.get("key"));
    }

    [Fact]
    public void URLSearchParams_sort_ShouldSortByKeys()
    {
        var params_ = new URLSearchParams("c=3&a=1&b=2");
        params_.sort();
        Assert.Equal("a=1&b=2&c=3", params_.ToString());
    }

    [Fact]
    public void URLSearchParams_keys_ShouldReturnAllKeys()
    {
        var params_ = new URLSearchParams("a=1&b=2&c=3");
        var keys = params_.keys().ToArray();
        Assert.Equal(new[] { "a", "b", "c" }, keys);
    }

    [Fact]
    public void URLSearchParams_values_ShouldReturnAllValues()
    {
        var params_ = new URLSearchParams("a=1&b=2&c=3");
        var values = params_.values().ToArray();
        Assert.Equal(new[] { "1", "2", "3" }, values);
    }

    [Fact]
    public void URLSearchParams_entries_ShouldReturnKeyValuePairs()
    {
        var params_ = new URLSearchParams("a=1&b=2");
        var entries = params_.entries().ToArray();
        Assert.Equal(2, entries.Length);
        Assert.Equal("a", entries[0].Key);
        Assert.Equal("1", entries[0].Value);
        Assert.Equal("b", entries[1].Key);
        Assert.Equal("2", entries[1].Value);
    }

    [Fact]
    public void URLSearchParams_forEach_ShouldIterateOverAllPairs()
    {
        var params_ = new URLSearchParams("a=1&b=2");
        var count = 0;
        params_.forEach((value, key) => count++);
        Assert.Equal(2, count);
    }

    [Fact]
    public void URLSearchParams_toString_ShouldReturnQueryString()
    {
        var params_ = new URLSearchParams("a=1&b=2");
        Assert.Equal("a=1&b=2", params_.ToString());
    }

    [Fact]
    public void URLSearchParams_toString_WithEmptyParams_ShouldReturnEmptyString()
    {
        var params_ = new URLSearchParams();
        Assert.Equal("", params_.ToString());
    }

    [Fact]
    public void URLSearchParams_toString_ShouldEncodeSpecialCharacters()
    {
        var params_ = new URLSearchParams();
        params_.append("key", "value with spaces");
        Assert.Contains("value+with+spaces", params_.ToString().Replace("%20", "+"));
    }

    [Fact]
    public void URLSearchParams_size_ShouldReturnCorrectCount()
    {
        var params_ = new URLSearchParams("a=1&b=2&c=3");
        Assert.Equal(3, params_.size);
    }

    [Fact]
    public void URLSearchParams_size_AfterModifications_ShouldUpdate()
    {
        var params_ = new URLSearchParams();
        Assert.Equal(0, params_.size);

        params_.append("a", "1");
        Assert.Equal(1, params_.size);

        params_.append("b", "2");
        Assert.Equal(2, params_.size);

        params_.delete("a");
        Assert.Equal(1, params_.size);
    }
}
