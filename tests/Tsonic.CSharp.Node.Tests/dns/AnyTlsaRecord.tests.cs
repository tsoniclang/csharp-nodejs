using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class AnyTlsaRecordTests
{
    [Fact]
    public void AnyTlsaRecord_HasCorrectType()
    {
        var record = new AnyTlsaRecord();
        Assert.Equal("TLSA", record.type);
    }
}
