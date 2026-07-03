using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class AnyARecordTests
{
    [Fact]
    public void AnyARecord_HasCorrectType()
    {
        var record = new AnyARecord();
        Assert.Equal("A", record.type);
    }
}
