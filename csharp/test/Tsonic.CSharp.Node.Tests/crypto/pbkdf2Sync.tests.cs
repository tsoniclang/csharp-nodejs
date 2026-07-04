using Xunit;
using System;
using System.Text;

namespace Tsonic.CSharp.Node.Tests;

public class pbkdf2SyncTests
{
    [Fact]
    public void pbkdf2Sync_GeneratesCorrectLength()
    {
        var derived = crypto.pbkdf2Sync("password", "salt", 1000, 32, "sha256");
        Assert.Equal(32, derived.Length);
    }

    [Fact]
    public void pbkdf2Sync_DeterministicOutput()
    {
        var derived1 = crypto.pbkdf2Sync("password", "salt", 1000, 32, "sha256");
        var derived2 = crypto.pbkdf2Sync("password", "salt", 1000, 32, "sha256");

        Assert.Equal(derived1, derived2);
    }

    [Fact]
    public void pbkdf2Sync_DifferentIterations()
    {
        var derived1 = crypto.pbkdf2Sync("password", "salt", 1000, 32, "sha256");
        var derived2 = crypto.pbkdf2Sync("password", "salt", 2000, 32, "sha256");

        Assert.NotEqual(derived1, derived2);
    }

    [Fact]
    public void pbkdf2Sync_DifferentSalts()
    {
        var derived1 = crypto.pbkdf2Sync("password", "salt1", 1000, 32, "sha256");
        var derived2 = crypto.pbkdf2Sync("password", "salt2", 1000, 32, "sha256");

        Assert.NotEqual(derived1, derived2);
    }

    [Fact]
    public void pbkdf2Sync_SHA1()
    {
        var derived = crypto.pbkdf2Sync("password", "salt", 1000, 32, "sha1");
        Assert.Equal(32, derived.Length);
    }

    [Fact]
    public void pbkdf2Sync_SHA384()
    {
        var derived = crypto.pbkdf2Sync("password", "salt", 1000, 32, "sha384");
        Assert.Equal(32, derived.Length);
    }

    [Fact]
    public void pbkdf2Sync_SHA512()
    {
        var derived = crypto.pbkdf2Sync("password", "salt", 1000, 32, "sha512");
        Assert.Equal(32, derived.Length);
    }

    [Fact]
    public void pbkdf2Sync_WithByteArrays()
    {
        var password = Encoding.UTF8.GetBytes("password");
        var salt = Encoding.UTF8.GetBytes("salt");
        var derived = crypto.pbkdf2Sync(password, salt, 1000, 32, "sha256");

        Assert.Equal(32, derived.Length);
    }
}
