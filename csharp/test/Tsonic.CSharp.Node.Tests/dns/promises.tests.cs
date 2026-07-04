using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class dnsPromisesTests
{
    [Fact]
    public async Task lookup_ShouldReturnAddress()
    {
        var result = await dns.promises.lookup("localhost");
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.address));
        Assert.True(result.family == 4 || result.family == 6);
    }

    [Fact]
    public async Task resolve_ShouldReturnRecords()
    {
        var records = await dns.promises.resolve("localhost");
        Assert.NotNull(records);
        Assert.NotEmpty(records);
    }
}
