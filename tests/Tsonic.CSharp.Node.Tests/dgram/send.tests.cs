using System;
using System.Text;
using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class sendTests
{
    [Fact]
    public void send_ToRemote_SendsMessage()
    {
        var server = dgram.createSocket("udp4");
        var messageReceived = false;
        byte[]? receivedData = null;

        server.on("message", (Action<byte[], RemoteInfo>)((data, rinfo) =>
        {
            messageReceived = true;
            receivedData = data;
        }));

        server.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var addr = server.address();
        var client = dgram.createSocket("udp4");

        var testMessage = "Hello UDP";
        client.send(testMessage, addr.port, "127.0.0.1");

        Thread.Sleep(200);

        Assert.True(messageReceived);
        Assert.NotNull(receivedData);
        Assert.Equal(testMessage, Encoding.UTF8.GetString(receivedData!));

        client.close();
        server.close();
    }

    [Fact]
    public void send_WithCallback_CallsCallback()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var addr = socket.address();
        var client = dgram.createSocket("udp4");

        var callbackCalled = false;
        Exception? callbackError = null;
        int bytesSent = 0;

        client.send("test", addr.port, "127.0.0.1", (err, bytes) =>
        {
            callbackCalled = true;
            callbackError = err;
            bytesSent = bytes;
        });

        Thread.Sleep(200);

        Assert.True(callbackCalled);
        Assert.Null(callbackError);
        Assert.Equal(4, bytesSent);

        client.close();
        socket.close();
    }

    [Fact]
    public void send_ConnectedSocket_SendsWithoutAddress()
    {
        var server = dgram.createSocket("udp4");
        var messageReceived = false;

        server.on("message", (Action<byte[], RemoteInfo>)((data, rinfo) =>
        {
            messageReceived = true;
        }));

        server.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var addr = server.address();
        var client = dgram.createSocket("udp4");
        client.connect(addr.port, "127.0.0.1");
        Thread.Sleep(100);

        client.send("test");
        Thread.Sleep(200);

        Assert.True(messageReceived);

        client.close();
        server.close();
    }

    [Fact]
    public void send_WithOffsetAndLength_SendsCorrectSlice()
    {
        var server = dgram.createSocket("udp4");
        byte[]? receivedData = null;

        server.on("message", (Action<byte[], RemoteInfo>)((data, rinfo) =>
        {
            receivedData = data;
        }));

        server.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var addr = server.address();
        var client = dgram.createSocket("udp4");

        var buffer = Encoding.UTF8.GetBytes("HelloWorld");
        client.send(buffer, 5, 5, addr.port, "127.0.0.1");

        Thread.Sleep(200);

        Assert.NotNull(receivedData);
        Assert.Equal("World", Encoding.UTF8.GetString(receivedData!));

        client.close();
        server.close();
    }

    [Fact]
    public void send_WithInvalidOffset_ThrowsException()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var buffer = new byte[10];
        Assert.Throws<ArgumentOutOfRangeException>(() => socket.send(buffer, 15, 5, 1234, "127.0.0.1"));

        socket.close();
    }

    [Fact]
    public void send_WithInvalidLength_ThrowsException()
    {
        var socket = dgram.createSocket("udp4");
        socket.bind(0, "127.0.0.1");
        Thread.Sleep(100);

        var buffer = new byte[10];
        Assert.Throws<ArgumentOutOfRangeException>(() => socket.send(buffer, 5, 10, 1234, "127.0.0.1"));

        socket.close();
    }
}
