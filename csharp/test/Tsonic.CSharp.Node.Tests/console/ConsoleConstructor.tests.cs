using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class ConsoleConstructorTests
{
    [Fact]
    public void Console_Property_ShouldBeAvailable()
    {
        var ctor = console.Console;
        Assert.NotNull(ctor);
        ctor.log("from constructor export");
    }

    [Fact]
    public void ConsoleConstructor_Instance_ShouldForwardMethods()
    {
        var instance = new ConsoleConstructor();
        instance.info("hello");
        instance.warn("warn");
        instance.error("error");
    }
}
