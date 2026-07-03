using System;
using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class Tls_constantsTests
{
    [Fact]
    public void constants_HaveExpectedValues()
    {
        Assert.Equal(3, tls.CLIENT_RENEG_LIMIT);
        Assert.Equal(600, tls.CLIENT_RENEG_WINDOW);
        Assert.Equal("auto", tls.DEFAULT_ECDH_CURVE);
        Assert.Equal("TLSv1.3", tls.DEFAULT_MAX_VERSION);
        Assert.Equal("TLSv1.2", tls.DEFAULT_MIN_VERSION);
    }
}
