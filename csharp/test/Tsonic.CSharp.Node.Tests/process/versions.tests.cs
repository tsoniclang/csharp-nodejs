using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class versionsTests
{
    [Fact]
    public void versions_ShouldReturnValidObject()
    {
        var versions = process.versions;

        Assert.NotNull(versions);
    }

    [Fact]
    public void versions_ShouldContainNodeVersion()
    {
        var versions = process.versions;

        Assert.NotNull(versions.node);
        Assert.NotEmpty(versions.node);
    }

    [Fact]
    public void versions_ShouldContainV8Version()
    {
        var versions = process.versions;

        Assert.NotNull(versions.v8);
        Assert.NotEmpty(versions.v8);
    }

    [Fact]
    public void versions_ShouldContainDotnetVersion()
    {
        var versions = process.versions;

        Assert.NotNull(versions.dotnet);
        Assert.NotEmpty(versions.dotnet);
    }

    [Fact]
    public void versions_ShouldContainTsonicVersion()
    {
        var versions = process.versions;

        Assert.NotNull(versions.tsonic);
        Assert.NotEmpty(versions.tsonic);
    }
}
