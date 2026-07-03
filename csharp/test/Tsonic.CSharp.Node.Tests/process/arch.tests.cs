using Xunit;
using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node.Tests;

public class archTests
{
    [Fact]
    public void arch_ShouldReturnValidArchitecture()
    {
        var arch = process.arch;

        Assert.NotNull(arch);
        Assert.NotEmpty(arch);

        // Should be one of the known architectures
        var validArchs = new[] { "x64", "ia32", "arm", "arm64", "wasm", "s390x" };
        Assert.Contains(arch, validArchs, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void arch_ShouldMatchProcessArchitecture()
    {
        var arch = process.arch;
        var expected = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "ia32",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            Architecture.Wasm => "wasm",
            Architecture.S390x => "s390x",
            _ => RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()
        };

        Assert.Equal(expected, arch);
    }
}
