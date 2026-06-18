using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class getCiphersTests
{
    [Fact]
    public void getCiphers_ReturnsNonEmptyList()
    {
        var ciphers = crypto.getCiphers();
        Assert.NotEmpty(ciphers);
        Assert.Contains("aes-256-cbc", ciphers);
    }

    [Fact]
    public void getCiphers_ContainsExpectedAlgorithms()
    {
        var ciphers = crypto.getCiphers();
        Assert.Contains("aes-128-cbc", ciphers);
        Assert.Contains("aes-192-cbc", ciphers);
        Assert.Contains("aes-256-cbc", ciphers);
        Assert.Contains("des-cbc", ciphers);
    }
}
