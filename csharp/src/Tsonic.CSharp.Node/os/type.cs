using System.Runtime.InteropServices;

namespace Tsonic.CSharp.Node;

public static partial class os
{
    /// <summary>
    /// Returns the operating system name as returned by uname(3).
    /// For example, it returns 'Linux' on Linux, 'Darwin' on macOS, and 'Windows_NT' on Windows.
    /// </summary>
    /// <returns>The operating system name.</returns>
    public static string type()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return "Windows_NT";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return "Linux";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return "Darwin";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            return "FreeBSD";

        return RuntimeInformation.OSDescription;
    }
}
