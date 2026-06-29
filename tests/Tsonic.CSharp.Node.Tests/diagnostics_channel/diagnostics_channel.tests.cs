using Tsonic.CSharp.Node;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class DiagnosticsChannelTests
{
    [Fact]
    public void Channel_PublishesToSubscribersAndUnsubscribes()
    {
        var channel = diagnostics_channel.channel("test.channel");
        object? observed = null;

        using (channel.subscribe((message, name) => observed = $"{name}:{message}"))
        {
            Assert.True(diagnostics_channel.hasSubscribers("test.channel"));
            channel.publish("value");
        }

        Assert.Equal("test.channel:value", observed);
        Assert.False(diagnostics_channel.hasSubscribers("test.channel"));
    }

    [Fact]
    public void ModuleSubscribe_ReturnsDisposableSubscription()
    {
        object? observed = null;
        using var subscription = diagnostics_channel.subscribe("module.channel", (message, _) => observed = message);

        diagnostics_channel.channel("module.channel").publish(123);

        Assert.Equal(123, observed);
        subscription.Dispose();
        Assert.False(diagnostics_channel.hasSubscribers("module.channel"));
    }

    [Fact]
    public void Unsubscribe_ReturnsFalseForMissingSubscriber()
    {
        var removed = diagnostics_channel.unsubscribe("missing.channel", (_, _) => { });

        Assert.False(removed);
    }

    [Fact]
    public void RunStores_ExecutesCallback()
    {
        var channel = diagnostics_channel.channel("runstores.channel");

        var result = channel.runStores("context", () => "done");

        Assert.Equal("done", result);
    }

    [Fact]
    public void Channel_RejectsEmptyName()
    {
        Assert.Throws<System.ArgumentException>(() => diagnostics_channel.channel(""));
    }
}
