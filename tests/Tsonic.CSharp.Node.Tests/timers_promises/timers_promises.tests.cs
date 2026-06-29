using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class TimersPromisesModuleTests
{
    [Fact]
    public async Task SetTimeout_ReturnsProvidedValue()
    {
        var result = await timers_promises.setTimeout(1, "done");

        Assert.Equal("done", result);
    }

    [Fact]
    public async Task SchedulerYield_Completes()
    {
        await timers_promises.scheduler.@yield();

        Assert.NotNull(timers_promises.scheduler);
    }
}
