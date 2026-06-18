using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class AnySrvRecordTests
{
    [Fact]
    public void AnySrvRecord_HasCorrectType()
    {
        var record = new AnySrvRecord();
        Assert.Equal("SRV", record.type);
    }
}
