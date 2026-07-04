using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class resolveTests
{
    [Fact]
    public void resolve_SimpleDomain_ReturnsAddresses()
    {
        var resetEvent = new ManualResetEventSlim(false);
        string[]? addresses = null;

        dns.resolve("localhost", (err, addrs) =>
        {
            addresses = addrs;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(addresses);
        Assert.True(addresses.Length > 0);
    }

    [Fact]
    public void resolve_WithARecordType_ReturnsIPv4Addresses()
    {
        var resetEvent = new ManualResetEventSlim(false);
        object? result = null;

        dns.resolve("localhost", "A", (err, res) =>
        {
            result = res;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(result);
        Assert.IsType<string[]>(result);
    }

    [Fact]
    public void resolve_WithAAAARecordType_ReturnsIPv6Addresses()
    {
        var resetEvent = new ManualResetEventSlim(false);
        object? result = null;

        dns.resolve("localhost", "AAAA", (err, res) =>
        {
            result = res;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(result);
        Assert.IsType<string[]>(result);
    }

    [Fact]
    public void resolve_WithMXRecordType_ReturnsMxRecords()
    {
        var resetEvent = new ManualResetEventSlim(false);
        object? result = null;

        dns.resolve("localhost", "MX", (err, res) =>
        {
            result = res;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(result);
        Assert.IsType<MxRecord[]>(result);
    }

    [Fact]
    public void resolve_WithTXTRecordType_ReturnsTxtRecords()
    {
        var resetEvent = new ManualResetEventSlim(false);
        object? result = null;

        dns.resolve("localhost", "TXT", (err, res) =>
        {
            result = res;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(result);
        Assert.IsType<string[][]>(result);
    }

    [Fact]
    public void resolve_WithInvalidRecordType_ReturnsError()
    {
        var resetEvent = new ManualResetEventSlim(false);
        Exception? error = null;

        dns.resolve("localhost", "INVALID", (err, res) =>
        {
            error = err;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(error);
    }
}
