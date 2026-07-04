using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class getHashesTests
{
    [Fact]
    public void getHashes_ReturnsNonEmptyList()
    {
        var hashes = crypto.getHashes();
        Assert.NotEmpty(hashes);
        Assert.Contains("sha256", hashes);
        Assert.Contains("md5", hashes);
    }

    [Fact]
    public void getHashes_ContainsExpectedAlgorithms()
    {
        var hashes = crypto.getHashes();
        Assert.Contains("md5", hashes);
        Assert.Contains("sha1", hashes);
        Assert.Contains("sha256", hashes);
        Assert.Contains("sha384", hashes);
        Assert.Contains("sha512", hashes);
    }
}
