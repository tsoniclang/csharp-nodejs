using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class hashTests
{
    [Fact]
    public void hash_Static_Works()
    {
        var data = Encoding.UTF8.GetBytes("test");
        var hash = crypto.hash("sha256", data);

        Assert.Equal(32, hash.Length);
    }

    [Fact]
    public void hash_StaticHash_Works()
    {
        var data = Encoding.UTF8.GetBytes("Test data");
        var hash = crypto.hash("sha256", data, null);

        Assert.Equal(32, hash.Length);
    }
}
