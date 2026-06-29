using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class TlsExtendedTests
{
    [Fact]
    public void DefaultPortAndCipherInfo_ShouldBeAvailable()
    {
        var info = new CipherInfo
        {
            name = "TLS_AES_128_GCM_SHA256",
            standardName = "TLS_AES_128_GCM_SHA256",
            version = "TLSv1.3"
        };

        Assert.Equal(443, tls.defaultPort);
        Assert.Equal("TLSv1.3", info.version);
    }
}
