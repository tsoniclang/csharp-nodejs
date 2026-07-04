using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class getDefaultAutoSelectFamilyTests
{
    [Fact]
    public void getDefaultAutoSelectFamily_ReturnsBoolean()
    {
        var value = net.getDefaultAutoSelectFamily();
        Assert.IsType<bool>(value);
    }
}
