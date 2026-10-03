using System;
using Microsoft.AspNetCore.Http;
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
        Assert.Equal("changed", view[0]);
        Assert.Equal("one", snapshot[0]);
        snapshot[1] = "independent";
        Assert.Equal("two", headers["x-item"]!.Value[1]);
        Assert.Null(headers["missing"]);
        dictionary["empty"] = StringValues.Empty;
        Assert.Null(headers["empty"]);
        Assert.Throws<TypeError>(() => { _ = headers["bad header"]; });
    }

    [Fact]
    public void IndexerDoesNotAllocateOrBoxValuesAndRetainsNullValidation()
    {
        var dictionary = new HeaderDictionary { ["x-item"] = new StringValues(new[] { "one", "two" }) };
        var headers = new IncomingHttpHeaders(dictionary);
        var total = 0;
        for (var index = 0; index < 1000; index++) total += headers["x-item"]!.Value.Count;
        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var index = 0; index < 10000; index++) total += headers["x-item"]!.Value.Count;
        Assert.Equal(0, GC.GetAllocatedBytesForCurrentThread() - before);
        Assert.Equal(22000, total);
        dictionary["invalid"] = new StringValues(new string?[] { "one", null });
        Assert.Throws<InvalidOperationException>(() => { _ = headers["invalid"]; });
    }
}
