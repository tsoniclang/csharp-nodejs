using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using Tsonic.CSharp.Node.Http;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class BorrowedHttpHeadersTests
{
    [Fact]
    public void IndexerBorrowsBackingWhileExplicitGetAllCopies()
    {
        var backing = new[] { "one", "two" };
        var dictionary = new HeaderDictionary { ["x-item"] = new StringValues(backing) };
        var headers = new IncomingHttpHeaders(dictionary);
        var view = headers["X-ITEM"] ?? throw new InvalidOperationException("Missing header.");
        var snapshot = headers.getAll("x-item");
        backing[0] = "changed";
        Assert.Equal("changed", HeaderValues.read(view, 0));
        Assert.Equal("one", snapshot[0]);
        snapshot[1] = "independent";
        Assert.Equal("two", HeaderValues.read(headers["x-item"] ?? throw new InvalidOperationException("Missing header."), 1));
        Assert.Null(headers["missing"]);
        dictionary["empty"] = StringValues.Empty;
        Assert.Null(headers["empty"]);
        Assert.Throws<TypeError>(() => { _ = headers["bad header"]; });
    }

    [Fact]
    public void IndexerDoesNotAllocateOrBoxValuesAndRetainsNullValidation()
    {
        var dictionary = new HeaderDictionary
        {
            ["x-item"] = new StringValues(new[] { "one", "two" }),
            ["invalid"] = new StringValues(new string?[] { "one", null }),
        };
        var headers = new IncomingHttpHeaders(dictionary);
        var total = 0;
        for (var index = 0; index < 1000; index++) total += (headers["x-item"] ?? throw new InvalidOperationException("Missing header.")).Count;
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 10000; index++) total += (headers["x-item"] ?? throw new InvalidOperationException("Missing header.")).Count;
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
        Assert.Equal(22000, total);
        var invalid = headers["invalid"] ?? throw new InvalidOperationException("Missing header.");
        Assert.Equal("one", HeaderValues.read(invalid, 0));
        Assert.Throws<InvalidOperationException>(() => HeaderValues.read(invalid, 1));
        Assert.Throws<InvalidOperationException>(() => headers.getAll("invalid"));
    }

    [Fact]
    public void IndexedReadChecksCurrentBackingWithoutPrevalidationOrBoxing()
    {
        var backing = new string?[] { "one", "two" };
        var dictionary = new HeaderDictionary { ["x-item"] = new StringValues(backing) };
        var headers = new IncomingHttpHeaders(dictionary);
        var view = headers["x-item"] ?? throw new InvalidOperationException("Missing header.");
        backing[1] = null;
        Assert.Throws<InvalidOperationException>(() => HeaderValues.read(view, 1));
        Assert.Equal("one", HeaderValues.read(view, 0));
        Assert.Throws<IndexOutOfRangeException>(() => HeaderValues.read(view, 2));
        for (var index = 0; index < 1000; index++) GC.KeepAlive(HeaderValues.read(view, 0));
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 10000; index++) GC.KeepAlive(HeaderValues.read(view, 0));
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
    }

    [Fact]
    public async Task AdmissionRetainsMetadataWhenTheNativeRequestFeaturesAreReused()
    {
        await using var connection = new DefaultConnectionContext();
        JsEventLoop.Run(() =>
        {
            var context = new DefaultHttpContext();
            context.Features.Set<IConnectionLifetimeFeature>(connection.Features.GetRequiredFeature<IConnectionLifetimeFeature>());
            context.Request.Method = "POST";
            context.Request.Path = "/first";
            context.Request.QueryString = new QueryString("?one=1");
            context.Request.Protocol = "HTTP/1.1";
            context.Request.Headers["x-request"] = "first";
            var first = new IncomingMessage(context.Request, Tsonic.CSharp.Js.JsEventLoop.Capture());
            context.Request.Headers.Clear();
            context.Request.Headers["x-request"] = "second";
            context.Request.Method = "GET";
            context.Request.Path = "/second";
            context.Request.QueryString = QueryString.Empty;
            context.Request.Protocol = "HTTP/2";
            var second = new IncomingMessage(context.Request, Tsonic.CSharp.Js.JsEventLoop.Capture());
            Assert.Equal("first", first.headers.get("x-request"));
            Assert.Equal("POST", first.method);
            Assert.Equal("/first?one=1", first.url);
            Assert.Equal("1.1", first.httpVersion);
            Assert.Equal("second", second.headers.get("x-request"));
            Assert.Equal("GET", second.method);
            Assert.Equal("/second", second.url);
            Assert.Equal("2", second.httpVersion);
            first.destroy();
            second.destroy();
        });
    }
}
