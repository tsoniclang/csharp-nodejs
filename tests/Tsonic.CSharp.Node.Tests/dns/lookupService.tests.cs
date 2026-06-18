using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class lookupServiceTests
{
    [Fact]
    public void lookupService_ValidIPAndPort_ReturnsHostnameAndService()
    {
        var resetEvent = new ManualResetEventSlim(false);
        string? hostname = null;
        string? service = null;
        Exception? error = null;

        dns.lookupService("127.0.0.1", 22, (err, host, svc) =>
        {
            error = err;
            hostname = host;
            service = svc;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.Null(error);
        Assert.NotNull(hostname);
        Assert.NotNull(service);
    }

    [Fact]
    public void lookupService_InvalidIP_ReturnsError()
    {
        var resetEvent = new ManualResetEventSlim(false);
        Exception? error = null;

        dns.lookupService("invalid-ip", 22, (err, host, svc) =>
        {
            error = err;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(error);
    }

    [Fact]
    public void lookupService_InvalidPort_ReturnsError()
    {
        var resetEvent = new ManualResetEventSlim(false);
        Exception? error = null;

        dns.lookupService("127.0.0.1", 99999, (err, host, svc) =>
        {
            error = err;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(error);
    }
}
