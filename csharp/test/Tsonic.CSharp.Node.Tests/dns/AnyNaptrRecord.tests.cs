using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class AnyNaptrRecordTests
{
    [Fact]
    public void AnyNaptrRecord_HasCorrectType()
    {
        var record = new AnyNaptrRecord();
        Assert.Equal("NAPTR", record.type);
    }
}
