using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class AnyAaaaRecordTests
{
    [Fact]
    public void AnyAaaaRecord_HasCorrectType()
    {
        var record = new AnyAaaaRecord();
        Assert.Equal("AAAA", record.type);
    }
}
