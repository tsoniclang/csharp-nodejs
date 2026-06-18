using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class reverseTests
{
    [Fact]
    public void reverse_ValidIPv4_ReturnsHostnames()
    {
        var resetEvent = new ManualResetEventSlim(false);
        string[]? hostnames = null;
        Exception? error = null;

        dns.reverse("127.0.0.1", (err, hosts) =>
        {
            error = err;
            hostnames = hosts;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.Null(error);
        Assert.NotNull(hostnames);
        Assert.True(hostnames.Length > 0);
    }

    [Fact]
    public void reverse_ValidIPv6_ReturnsHostnames()
    {
        var resetEvent = new ManualResetEventSlim(false);
        string[]? hostnames = null;
        Exception? error = null;

        dns.reverse("::1", (err, hosts) =>
        {
            error = err;
            hostnames = hosts;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.Null(error);
        Assert.NotNull(hostnames);
    }

    [Fact]
    public void reverse_InvalidIP_ReturnsError()
    {
        var resetEvent = new ManualResetEventSlim(false);
        Exception? error = null;

        dns.reverse("invalid-ip", (err, hosts) =>
        {
            error = err;
            resetEvent.Set();
        });

        resetEvent.Wait(5000);
        Assert.NotNull(error);
    }
}
