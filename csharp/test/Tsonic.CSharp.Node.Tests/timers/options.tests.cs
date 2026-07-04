using System.Threading;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class TimerOptionsTests
{
    [Fact]
    public void TimerOptions_ShouldExposeSignalState()
    {
        using var source = new CancellationTokenSource();
        var options = new TimerOptions { signal = source.Token };

        Assert.False(options.signalAborted);
        source.Cancel();
        Assert.True(options.signalAborted);
    }
}
