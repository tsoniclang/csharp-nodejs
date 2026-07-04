using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class DnsPromisesModuleTests
{
    [Fact]
    public async Task LookupLocalhost_ReturnsAddress()
    {
        var result = await dns_promises.lookup("localhost");

        Assert.False(string.IsNullOrWhiteSpace(result.address));
        Assert.True(result.family is 4 or 6);
    }

    [Fact]
    public async Task LookupAllLocalhost_ReturnsAddresses()
    {
        var result = await dns_promises.lookupAll("localhost");

        Assert.NotEmpty(result);
        Assert.All(result, item => Assert.True(item.family is 4 or 6));
    }
}
