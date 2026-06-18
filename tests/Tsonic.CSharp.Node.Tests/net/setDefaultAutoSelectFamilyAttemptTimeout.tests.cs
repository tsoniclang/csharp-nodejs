using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class setDefaultAutoSelectFamilyAttemptTimeoutTests
{
    [Fact]
    public void setDefaultAutoSelectFamilyAttemptTimeout_UpdatesValue()
    {
        var original = net.getDefaultAutoSelectFamilyAttemptTimeout();
        net.setDefaultAutoSelectFamilyAttemptTimeout(500);
        Assert.Equal(500, net.getDefaultAutoSelectFamilyAttemptTimeout());
        // Reset to original
        net.setDefaultAutoSelectFamilyAttemptTimeout(original);
    }
}
