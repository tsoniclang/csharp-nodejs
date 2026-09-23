using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class JsNumericTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public void IntegerInputPreservesNativeValues(int value)
    {
        Assert.Equal(value, JsNumeric.RequireInteger(value));
    }

    [Theory]
    [InlineData(0.5)]
    [InlineData(-0.5)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void IntegerInputRejectsNonintegralValues(double value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => JsNumeric.RequireInteger(value));
    }

    [Theory]
    [InlineData(2147483648d)]
    [InlineData(-2147483649d)]
    public void IntegerInputRejectsNativeOverflow(double value)
    {
        Assert.Throws<OverflowException>(() => JsNumeric.RequireInteger(value));
    }

}
