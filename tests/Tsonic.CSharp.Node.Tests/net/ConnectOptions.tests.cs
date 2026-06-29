using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class ConnectOptionsTests
{
    [Fact]
    public void ConnectOptions_ShouldInitializePortAndHost()
    {
        var options = new ConnectOptions(8080, "localhost");

        Assert.Equal(8080, options.port);
        Assert.Equal("localhost", options.host);
    }

    [Fact]
    public void LookupEndpoint_ShouldClassifyAddressFamily()
    {
        var endpoint = net.lookupEndpoint("127.0.0.1");

        Assert.Equal("127.0.0.1", endpoint.address);
        Assert.Equal(4, endpoint.family);
    }
}
