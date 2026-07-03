using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class getCurvesTests
{
    [Fact]
    public void getCurves_ReturnsNonEmptyList()
    {
        var curves = crypto.getCurves();
        Assert.NotEmpty(curves);
        Assert.Contains("secp256r1", curves);
    }

    [Fact]
    public void getCurves_ContainsExpectedCurves()
    {
        var curves = crypto.getCurves();
        Assert.Contains("secp256r1", curves);
        Assert.Contains("secp384r1", curves);
        Assert.Contains("secp521r1", curves);
    }
}
