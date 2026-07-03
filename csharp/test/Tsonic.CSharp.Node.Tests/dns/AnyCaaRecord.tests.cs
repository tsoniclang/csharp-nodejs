using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class AnyCaaRecordTests
{
    [Fact]
    public void AnyCaaRecord_HasCorrectType()
    {
        var record = new AnyCaaRecord();
        Assert.Equal("CAA", record.type);
    }
}
