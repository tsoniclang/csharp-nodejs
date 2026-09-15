using Xunit;

namespace Tsonic.CSharp.Node.Tests;

public class V8FlagsTests
{
    [Theory]
    [InlineData("--stack_size=1024")]
    [InlineData("")]
    [InlineData("--trace_gc")]
    public void NativeFlagsRemainUnsupportedOnRepeatedInvocation(string flags)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            var error = Assert.Throws<System.PlatformNotSupportedException>(() => v8.setFlagsFromString(flags));
            Assert.Equal(
                "node:v8.setFlagsFromString requires a V8 engine; native programs do not host V8",
                error.Message);
        }
    }
}
