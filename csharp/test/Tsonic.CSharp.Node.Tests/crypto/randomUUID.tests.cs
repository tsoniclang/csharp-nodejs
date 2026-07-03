using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class randomUUIDTests
{
    [Fact]
    public void randomUUID_GeneratesValidUUID()
    {
        var uuid = crypto.randomUUID();
        Assert.Matches(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", uuid);
    }
}
