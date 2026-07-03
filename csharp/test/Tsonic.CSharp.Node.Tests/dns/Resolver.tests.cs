using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class ResolverTests
{
    [Fact]
    public void Resolver_Constructor_CreatesInstance()
    {
        var resolver = new Resolver();
        Assert.NotNull(resolver);
    }

    [Fact]
    public void Resolver_ConstructorWithOptions_CreatesInstance()
    {
        var options = new ResolverOptions
        {
            timeout = 5000,
            tries = 3
        };
        var resolver = new Resolver(options);
        Assert.NotNull(resolver);
    }

    [Fact]
    public void Resolver_Cancel_DoesNotThrow()
    {
        var resolver = new Resolver();
        var exception = Record.Exception(() =>
        {
            resolver.cancel();
        });
        Assert.Null(exception);
    }

    [Fact]
    public void Resolver_Cancel_SubsequentCallsReturnCancelledError()
    {
        var resolver = new Resolver();
        resolver.cancel();

        var resetEvent = new ManualResetEventSlim(false);
        Exception? error = null;

        resolver.resolve4("localhost", (err, addrs) =>
        {
            error = err;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(error);
        Assert.Contains("ECANCELLED", error.Message);
    }

    [Fact]
    public void Resolver_Resolve4_WorksLikeStaticMethod()
    {
        var resolver = new Resolver();
        var resetEvent = new ManualResetEventSlim(false);
        string[]? addresses = null;

        resolver.resolve4("localhost", (err, addrs) =>
        {
            addresses = addrs;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(addresses);
    }

    [Fact]
    public void Resolver_Resolve6_WorksLikeStaticMethod()
    {
        var resolver = new Resolver();
        var resetEvent = new ManualResetEventSlim(false);
        string[]? addresses = null;

        resolver.resolve6("localhost", (err, addrs) =>
        {
            addresses = addrs;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(addresses);
    }

    [Fact]
    public void Resolver_ResolveMx_WorksLikeStaticMethod()
    {
        var resolver = new Resolver();
        var resetEvent = new ManualResetEventSlim(false);
        MxRecord[]? records = null;

        resolver.resolveMx("localhost", (err, recs) =>
        {
            records = recs;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(records);
    }

    [Fact]
    public void Resolver_Reverse_WorksLikeStaticMethod()
    {
        var resolver = new Resolver();
        var resetEvent = new ManualResetEventSlim(false);
        string[]? hostnames = null;

        resolver.reverse("127.0.0.1", (err, hosts) =>
        {
            hostnames = hosts;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(hostnames);
    }

    [Fact]
    public void Resolver_SetLocalAddress_DoesNotThrow()
    {
        var resolver = new Resolver();
        var exception = Record.Exception(() =>
        {
            resolver.setLocalAddress("0.0.0.0", "::0");
        });
        Assert.Null(exception);
    }

    [Fact]
    public void Resolver_GetServers_ReturnsArray()
    {
        var resolver = new Resolver();
        var servers = resolver.getServers();
        Assert.NotNull(servers);
        Assert.IsType<string[]>(servers);
    }

    [Fact]
    public void Resolver_SetServers_DoesNotThrow()
    {
        var resolver = new Resolver();
        var exception = Record.Exception(() =>
        {
            resolver.setServers(new[] { "8.8.8.8" });
        });
        Assert.Null(exception);
    }
}
