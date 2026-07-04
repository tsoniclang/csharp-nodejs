using System;
using System.Text;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class createSocketTests
{
    [Fact]
    public void createSocket_UDP4_CreatesSocket()
    {
        var socket = dgram.createSocket("udp4");
        Assert.NotNull(socket);
        socket.close();
    }

    [Fact]
    public void createSocket_UDP6_CreatesSocket()
    {
        var socket = dgram.createSocket("udp6");
        Assert.NotNull(socket);
        socket.close();
    }

    [Fact]
    public void createSocket_WithCallback_AttachesMessageListener()
    {
        var messageReceived = false;
        byte[]? receivedData = null;
        RemoteInfo? receivedInfo = null;

        var socket = dgram.createSocket("udp4", (data, rinfo) =>
        {
            messageReceived = true;
            receivedData = data;
            receivedInfo = rinfo;
        });

        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100); // Wait for bind

        var addr = socket.address();
        var client = dgram.createSocket("udp4");

        client.send("test", addr.port, "127.0.0.1", (err, bytes) =>
        {
            client.close();
        });

        Thread.Sleep(200); // Wait for message

        Assert.True(messageReceived);
        Assert.NotNull(receivedData);
        Assert.Equal("test", Encoding.UTF8.GetString(receivedData!));
        Assert.NotNull(receivedInfo);
        Assert.Equal("127.0.0.1", receivedInfo!.address);

        socket.close();
    }
}
