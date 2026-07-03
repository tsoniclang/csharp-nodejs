using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class CipherNameAndProtocolTests
{
    [Fact]
    public void CipherNameAndProtocol_AllProperties_CanBeSet()
    {
        var cipher = new CipherNameAndProtocol
        {
            name = "AES256-GCM-SHA384",
            version = "TLSv1.3",
            standardName = "TLS_AES_256_GCM_SHA384"
        };

        Assert.Equal("AES256-GCM-SHA384", cipher.name);
        Assert.Equal("TLSv1.3", cipher.version);
    }
}
