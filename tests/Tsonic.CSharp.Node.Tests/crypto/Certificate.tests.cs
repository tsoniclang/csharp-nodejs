using Xunit;
using System;

namespace Tsonic.CSharp.Node.Tests;

public class CertificateTests
{
    [Fact]
    public void Certificate_ExportChallenge_String_ThrowsNotImplemented()
    {
        Assert.Throws<NotImplementedException>(() => Certificate.exportChallenge("test"));
    }

    [Fact]
    public void Certificate_ExportChallenge_Bytes_ThrowsNotImplemented()
    {
        Assert.Throws<NotImplementedException>(() => Certificate.exportChallenge(new byte[64]));
    }

    [Fact]
    public void Certificate_ExportPublicKey_String_ThrowsNotImplemented()
    {
        Assert.Throws<NotImplementedException>(() => Certificate.exportPublicKey("test"));
    }

    [Fact]
    public void Certificate_ExportPublicKey_Bytes_ThrowsNotImplemented()
    {
        Assert.Throws<NotImplementedException>(() => Certificate.exportPublicKey(new byte[64]));
    }

    [Fact]
    public void Certificate_VerifySpkac_String_ThrowsNotImplemented()
    {
        Assert.Throws<NotImplementedException>(() => Certificate.verifySpkac("test"));
    }

    [Fact]
    public void Certificate_VerifySpkac_Bytes_ThrowsNotImplemented()
    {
        Assert.Throws<NotImplementedException>(() => Certificate.verifySpkac(new byte[64]));
    }
}
