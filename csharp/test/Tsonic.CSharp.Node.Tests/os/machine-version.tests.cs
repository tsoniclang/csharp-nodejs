using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class osMachineVersionTests
{
    [Fact]
    public void machine_ShouldReturnNonEmptyValue()
    {
        var value = os.machine();
        Assert.False(string.IsNullOrWhiteSpace(value));
    }

    [Fact]
    public void version_ShouldReturnNonEmptyValue()
    {
        var value = os.version();
        Assert.False(string.IsNullOrWhiteSpace(value));
    }
}
