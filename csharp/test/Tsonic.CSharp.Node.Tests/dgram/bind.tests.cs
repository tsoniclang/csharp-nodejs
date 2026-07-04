using System;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class bindTests
{
    [Fact]
    public void bind_WithPort_BindsSuccessfully()
    {
        var socket = dgram.createSocket("udp4");
        var listening = false;

        socket.on("listening", (Action)(() =>
        {
            listening = true;
        }));

        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        Assert.True(listening);
        var addr = socket.address();
        Assert.Equal("127.0.0.1", addr.address);
        Assert.True(addr.port > 0);

        socket.close();
    }

    [Fact]
    public void bind_WithCallback_CallsCallback()
    {
        var socket = dgram.createSocket("udp4");
        var callbackCalled = false;

        socket.bind(0, "127.0.0.1", () =>
        {
            callbackCalled = true;
        });

        Thread.Sleep(100);
        Assert.True(callbackCalled);

        socket.close();
    }

    [Fact]
    public void bind_WithBindOptions_BindsSuccessfully()
    {
        var socket = dgram.createSocket("udp4");

        var options = new BindOptions
        {
            port = 0,
            address = "127.0.0.1"
        };

        socket.bind(options);
        Thread.Sleep(100);

        var addr = socket.address();
        Assert.Equal("127.0.0.1", addr.address);
        Assert.True(addr.port > 0);

        socket.close();
    }

    [Fact]
    public void bind_WithBindOptionsAndCallback_CallsCallback()
    {
        var socket = dgram.createSocket("udp4");
        bool callbackCalled = false;

        var options = new BindOptions
        {
            port = 0,
            address = "127.0.0.1"
        };

        socket.bind(options, () => { callbackCalled = true; });
        Thread.Sleep(100);

        Assert.True(callbackCalled);

        socket.close();
    }

    [Fact]
    public void bind_WithFileDescriptor_ThrowsNotSupported()
    {
        var socket = dgram.createSocket("udp4");

        var options = new BindOptions
        {
            fd = 123
        };

        Assert.Throws<NotSupportedException>(() => socket.bind(options));

        socket.close();
    }
}
