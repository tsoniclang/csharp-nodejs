using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class createDecipherivTests
{
    [Fact]
    public void createDecipheriv_SetAuthTag_ThrowsNotSupportedForNonAuthenticatedMode()
    {
        var decipher = crypto.createDecipheriv("aes-256-cbc", crypto.randomBytes(32), crypto.randomBytes(16));

        // setAuthTag only works with authenticated modes like GCM
        Assert.Throws<NotSupportedException>(() => decipher.setAuthTag(new byte[16]));
    }

    [Fact]
    public void createDecipheriv_SetAAD_ThrowsNotSupportedForNonAuthenticatedMode()
    {
        var decipher = crypto.createDecipheriv("aes-256-cbc", crypto.randomBytes(32), crypto.randomBytes(16));

        // setAAD only works with authenticated modes like GCM
        Assert.Throws<NotSupportedException>(() => decipher.setAAD(Encoding.UTF8.GetBytes("additional data")));
    }
}
