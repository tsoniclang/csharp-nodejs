using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class randomIntTests
{
    [Fact]
    public void randomInt_GeneratesWithinRange()
    {
        for (int i = 0; i < 100; i++)
        {
            var value = crypto.randomInt(10);
            Assert.InRange(value, 0, 9);
        }
    }

    [Fact]
    public void randomInt_GeneratesWithinCustomRange()
    {
        for (int i = 0; i < 100; i++)
        {
            var value = crypto.randomInt(5, 15);
            Assert.InRange(value, 5, 14);
        }
    }
}
