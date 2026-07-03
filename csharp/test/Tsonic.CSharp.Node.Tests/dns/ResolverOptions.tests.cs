using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class ResolverOptionsTests
{
    [Fact]
    public void ResolverOptions_AllProperties_CanBeSet()
    {
        var options = new ResolverOptions
        {
            timeout = 5000,
            tries = 3,
            maxTimeout = 10000
        };

        Assert.Equal(5000, options.timeout);
        Assert.Equal(3, options.tries);
        Assert.Equal(10000, options.maxTimeout);
    }
}
