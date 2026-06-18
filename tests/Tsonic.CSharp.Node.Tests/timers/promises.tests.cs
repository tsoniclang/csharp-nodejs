using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class timersPromisesTests
{
    [Fact]
    public async Task setTimeout_ShouldResolveValue()
    {
        var value = await timers.promises.setTimeout(10, "ok");
        Assert.Equal("ok", value);
    }

    [Fact]
    public async Task setImmediate_ShouldResolveValue()
    {
        var value = await timers.promises.setImmediate(123);
        Assert.Equal(123, value);
    }

    [Fact]
    public async Task setInterval_ShouldYieldValues()
    {
        await foreach (var value in timers.promises.setInterval(1, "tick"))
        {
            Assert.Equal("tick", value);
            break;
        }
    }

    [Fact]
    public async Task scheduler_wait_ShouldComplete()
    {
        await timers.promises.scheduler.wait(1);
    }
}
