using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class dnsPromisesTests
{
    [Fact]
    public void lookup_ShouldReturnAddress() => Tsonic.CSharp.Js.JsEventLoop.Run(async () =>
    {
        var result = await dns.promises.lookup("localhost");
        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.address));
        Assert.True(result.family == 4 || result.family == 6);
    });

    [Fact]
    public void resolve_ShouldReturnRecords() => Tsonic.CSharp.Js.JsEventLoop.Run(async () =>
    {
        var records = await dns.promises.resolve("localhost");
        Assert.NotNull(records);
        Assert.NotEmpty(records);
    });
}
