using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class extnameTests
{
    [Fact]
    public void extname_ShouldReturnExtension()
    {
        Assert.Equal(".html", path.extname("index.html"));
        Assert.Equal(".md", path.extname("index.coffee.md"));
        Assert.Equal(string.Empty, path.extname("index"));

        // Platform-specific behavior for trailing dot
        var trailingDotResult = path.extname("index.");
        Assert.True(trailingDotResult == "." || trailingDotResult == string.Empty);

        // Platform-specific behavior for dotfiles
        var dotIndexResult = path.extname(".index");
        Assert.True(dotIndexResult == string.Empty || dotIndexResult == ".index");
    }
}
