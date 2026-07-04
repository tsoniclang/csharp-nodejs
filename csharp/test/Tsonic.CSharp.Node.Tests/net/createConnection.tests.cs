using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class createConnectionTests
{
    private const int TEST_PORT = 18234;

    [Fact]
    public void createConnection_CreatesSocket()
    {
        var socket = net.createConnection(TEST_PORT, "localhost");
        Assert.NotNull(socket);
        Assert.IsType<Socket>(socket);
    }
}
