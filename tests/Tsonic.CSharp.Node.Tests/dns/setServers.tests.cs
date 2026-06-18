using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class setServersTests
{
    [Fact]
    public void setServers_ValidServers_DoesNotThrow()
    {
        var exception = Record.Exception(() =>
        {
            dns.setServers(new[] { "8.8.8.8", "8.8.4.4" });
        });
        Assert.Null(exception);
    }
}
