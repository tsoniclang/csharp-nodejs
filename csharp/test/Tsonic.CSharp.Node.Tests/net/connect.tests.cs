using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class Net_connectTests
{
    private const int TEST_PORT = 18234;

    [Fact]
    public void connect_CreatesSocket()
    {
        var socket = net.connect(TEST_PORT, "localhost");
        Assert.NotNull(socket);
        Assert.IsType<Socket>(socket);
    }
}
