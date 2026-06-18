using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class getDefaultAutoSelectFamilyAttemptTimeoutTests
{
    [Fact]
    public void getDefaultAutoSelectFamilyAttemptTimeout_ReturnsInt()
    {
        var value = net.getDefaultAutoSelectFamilyAttemptTimeout();
        Assert.IsType<int>(value);
        Assert.True(value >= 0);
    }
}
