using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class UtilMoreTests
{
    [Fact]
    public void StyleTextAndSystemErrors_ShouldExposeClosedData()
    {
        Assert.Equal("\u001b[31mred\u001b[39m", util.styleText("red", "red"));
        Assert.Equal("ENOENT", util.getSystemErrorName(2));
        Assert.Contains(13, util.getSystemErrorMap().Keys);
    }

    [Fact]
    public async Task PromisifyAndCallbackify_ShouldBridgeClosedDelegates()
    {
        var asyncValue = util.promisify(() => 42);
        Assert.Equal(42, await asyncValue());

        var called = false;
        var callback = util.callbackify(() =>
        {
            called = true;
            return Task.CompletedTask;
        });
        callback(Task.CompletedTask);

        Assert.True(called);
    }

    [Fact]
    public async Task Aborted_ShouldCompleteWhenTokenIsCanceled()
    {
        using var source = new CancellationTokenSource();
        var task = util.aborted(source.Token);
        source.Cancel();

        Assert.True(await task);
    }

    [Fact]
    public void CallSitesDiffAndSignalConversion_ShouldWork()
    {
        var sites = util.getCallSites();
        var diff = util.diff("a", "b");

        Assert.NotEmpty(sites);
        Assert.Equal(["delete", "insert"], [diff[0].type, diff[1].type]);
        Assert.Equal(130, util.convertProcessSignalToExitCode("SIGINT"));
    }
}
