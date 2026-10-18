using System.Threading.Tasks;
using Tsonic.CSharp.Js;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class DnsPromisesModuleTests
{
    [Fact]
    public void LookupLocalhost_ReturnsAddress() => JsEventLoop.Run(async () =>
    {
        var result = await dns_promises.lookup("localhost");

        Assert.False(string.IsNullOrWhiteSpace(result.address));
        Assert.True(result.family is 4 or 6);
    });

    [Fact]
    public void LookupAllLocalhost_ReturnsAddresses() => JsEventLoop.Run(async () =>
    {
        var result = await dns_promises.lookupAll("localhost");

        Assert.NotEmpty(result);
        Assert.All(result, item => Assert.True(item.family is 4 or 6));
    });
}
