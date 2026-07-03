using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node;

public static partial class os
{
    /// <summary>
    /// The platform-specific path to the null device.
    /// /dev/null on POSIX
    /// \\.\nul on Windows
    /// </summary>
    public static readonly string devNull = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? "\\\\.\\nul"
        : "/dev/null";
}
