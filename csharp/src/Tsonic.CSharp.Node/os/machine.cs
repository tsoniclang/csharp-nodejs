using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node;

public static partial class os
{
    /// <summary>
    /// Returns the machine type as an identifier like x86_64 or arm64.
    /// </summary>
    public static string machine()
    {
        return RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x86_64",
            Architecture.X86 => "i686",
            Architecture.Arm64 => "arm64",
            Architecture.Arm => "arm",
            _ => RuntimeInformation.OSArchitecture.ToString().ToLowerInvariant(),
        };
    }
}
