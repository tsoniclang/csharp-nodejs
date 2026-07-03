using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class getDiffieHellmanTests
{
    [Fact]
    public void getDiffieHellman_CreatesGroupInstance()
    {
        var dh = crypto.getDiffieHellman("modp1");

        Assert.NotNull(dh);
        Assert.NotNull(dh.getPrime());
        Assert.NotNull(dh.getGenerator());
    }
}
