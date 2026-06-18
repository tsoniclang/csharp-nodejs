using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node;

public static partial class process
{
    /// <summary>
    /// The operating system CPU architecture for which the Node.js binary was compiled.
    /// Possible values are: 'arm', 'arm64', 'ia32', 'loong64', 'mips', 'mipsel', 'ppc', 'ppc64', 'riscv64', 's390', 's390x', and 'x64'.
    /// </summary>
    public static string arch
    {
        get
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X64 => "x64",
                Architecture.X86 => "ia32",
                Architecture.Arm => "arm",
                Architecture.Arm64 => "arm64",
                Architecture.Wasm => "wasm",
                Architecture.S390x => "s390x",
                _ => RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()
            };
        }
    }
}
