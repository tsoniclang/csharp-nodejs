using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class toNamespacedPathTests
{
    [Fact]
    public void toNamespacedPath_ShouldHandleEmptyPath()
    {
        Assert.Equal(string.Empty, path.toNamespacedPath(""));
    }
}
